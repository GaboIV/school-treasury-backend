using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly FirebaseMessaging _firebaseMessaging;

        public FirebaseNotificationService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<FirebaseNotificationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;

            // Initialize Firebase Admin SDK if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    var credentialPath = configuration["Firebase:CredentialJson"];
                    _logger.LogInformation("Ruta del archivo de credenciales Firebase: " + credentialPath);

                    if (!File.Exists(credentialPath))
                    {
                        throw new FileNotFoundException($"El archivo de credenciales de Firebase no se encontró en {credentialPath}");
                    }

                    // Leer el contenido del archivo
                    var credentialJson = File.ReadAllText(credentialPath);

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(credentialJson)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing Firebase Admin SDK");
                    throw;
                }
            }

            _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<bool> AddTokenAsync(string userId, string fcmToken)
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogWarning("Se intentó añadir un token FCM vacío para el usuario {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Iniciando proceso de registro de token FCM para usuario {UserId}", userId);
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario {UserId} para registrar token FCM", userId);
                    return false;
                }

                _logger.LogInformation("Usuario {UserId} encontrado. Rol: {Role}, TokensActuales: {TokenCount}", 
                    userId, user.Role, user.FcmTokens?.Count ?? 0);
                
                // Verificar si FcmTokens es nulo y inicializarlo si es necesario
                if (user.FcmTokens == null)
                {
                    _logger.LogInformation("Inicializando lista de tokens FCM para el usuario {UserId}", userId);
                    user.FcmTokens = new List<string>();
                }

                // Add token if it doesn't exist
                if (!user.FcmTokens.Contains(fcmToken))
                {
                    _logger.LogInformation("El token FCM es nuevo para el usuario {UserId}, añadiendo: {TokenPrefix}...", 
                        userId, fcmToken.Length > 10 ? fcmToken.Substring(0, 10) + "..." : fcmToken);
                    
                    user.FcmTokens.Add(fcmToken);
                    
                    _logger.LogInformation("Actualizando usuario {UserId} en el repositorio", userId);
                    var updateResult = await _userRepository.UpdateAsync(user);
                    if (!updateResult)
                    {
                        _logger.LogError("Error al actualizar el usuario {UserId} en el repositorio", userId);
                        return false;
                    }
                    
                    _logger.LogInformation("Token FCM añadido y usuario actualizado. Suscribiendo a tópicos...");

                    // Subscribe to topics based on user role with error handling
                    bool subscriptionSuccess = true;
                    
                    // Intentar suscribir a General
                    var generalResult = await SubscribeToTopicAsync(fcmToken, "General");
                    if (generalResult)
                    {
                        _logger.LogInformation("Token suscrito exitosamente al tópico General");
                    }
                    else
                    {
                        _logger.LogWarning("Falló la suscripción al tópico General, pero continuaremos con el proceso");
                        subscriptionSuccess = false;
                    }
                    
                    // Suscribir a tópicos adicionales según el rol
                    if (user.Role == UserRole.Representative)
                    {
                        var representativeResult = await SubscribeToTopicAsync(fcmToken, "Representative");
                        if (representativeResult)
                        {
                            _logger.LogInformation("Token suscrito exitosamente al tópico Representative");
                        }
                        else
                        {
                            _logger.LogWarning("Falló la suscripción al tópico Representative, pero continuaremos con el proceso");
                            subscriptionSuccess = false;
                        }
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        var adminResult = await SubscribeToTopicAsync(fcmToken, "Admin");
                        if (adminResult)
                        {
                            _logger.LogInformation("Token suscrito exitosamente al tópico Admin");
                        }
                        else
                        {
                            _logger.LogWarning("Falló la suscripción al tópico Admin, pero continuaremos con el proceso");
                            subscriptionSuccess = false;
                        }
                    }

                    if (!subscriptionSuccess)
                    {
                        _logger.LogWarning("El token FCM se añadió al usuario {UserId} pero hubo problemas con algunas suscripciones a tópicos", userId);
                    }
                }
                else
                {
                    _logger.LogInformation("El token FCM ya existe para el usuario {UserId}, no es necesario añadirlo nuevamente", userId);
                    
                    // Aunque el token ya exista, intentar suscribirlo para asegurar que esté suscrito a todos los tópicos necesarios
                    _logger.LogInformation("Verificando suscripciones a tópicos para token existente...");
                    
                    await SubscribeToTopicAsync(fcmToken, "General");
                    
                    if (user.Role == UserRole.Representative)
                    {
                        await SubscribeToTopicAsync(fcmToken, "Representative");
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        await SubscribeToTopicAsync(fcmToken, "Admin");
                    }
                }

                _logger.LogInformation("Proceso de registro de token FCM completado para el usuario {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al añadir token FCM para el usuario {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveTokenAsync(string userId, string fcmToken)
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogWarning("Se intentó eliminar un token FCM vacío para el usuario {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Iniciando proceso de eliminación de token FCM para usuario {UserId}", userId);
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario {UserId} para eliminar token FCM", userId);
                    return false;
                }

                _logger.LogInformation("Usuario {UserId} encontrado. Verificando si tiene el token FCM", userId);
                
                // Verificar si FcmTokens es nulo
                if (user.FcmTokens == null)
                {
                    _logger.LogInformation("La lista de tokens FCM es nula para el usuario {UserId}, nada que eliminar", userId);
                    return true; // Ya no existe, consideramos que se eliminó correctamente
                }

                // Remove token if it exists
                if (user.FcmTokens.Contains(fcmToken))
                {
                    _logger.LogInformation("Token FCM encontrado para el usuario {UserId}, procediendo a eliminar", userId);
                    
                    // Intentar desuscribir de los tópicos primero
                    _logger.LogInformation("Desuscribiendo token de tópicos antes de eliminarlo");
                    bool unsubscribeSuccess = true;
                    
                    // Intentar desuscribir de General
                    var generalResult = await UnsubscribeFromTopicAsync(fcmToken, "General");
                    if (!generalResult)
                    {
                        _logger.LogWarning("Falló la desuscripción del tópico General, pero continuaremos con el proceso");
                        unsubscribeSuccess = false;
                    }
                    
                    // Desuscribir de tópicos adicionales según el rol
                    if (user.Role == UserRole.Representative)
                    {
                        var representativeResult = await UnsubscribeFromTopicAsync(fcmToken, "Representative");
                        if (!representativeResult)
                        {
                            _logger.LogWarning("Falló la desuscripción del tópico Representative, pero continuaremos con el proceso");
                            unsubscribeSuccess = false;
                        }
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        var adminResult = await UnsubscribeFromTopicAsync(fcmToken, "Admin");
                        if (!adminResult)
                        {
                            _logger.LogWarning("Falló la desuscripción del tópico Admin, pero continuaremos con el proceso");
                            unsubscribeSuccess = false;
                        }
                    }

                    if (!unsubscribeSuccess)
                    {
                        _logger.LogWarning("Hubo problemas al desuscribir el token de algunos tópicos, pero continuaremos con la eliminación");
                    }

                    // Eliminar el token de la lista del usuario
                    user.FcmTokens.Remove(fcmToken);
                    _logger.LogInformation("Token eliminado de la lista. Actualizando usuario {UserId} en el repositorio", userId);
                    
                    var updateResult = await _userRepository.UpdateAsync(user);
                    if (!updateResult)
                    {
                        _logger.LogError("Error al actualizar el usuario {UserId} en el repositorio después de eliminar el token", userId);
                        return false;
                    }
                    
                    _logger.LogInformation("Token FCM eliminado exitosamente para el usuario {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("El token FCM no existe para el usuario {UserId}, nada que eliminar", userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar token FCM para el usuario {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SubscribeToTopicAsync(string token, string topic)
        {
            _logger.LogInformation("Intentando suscribir el token {Token} al tópico {Topic}", token, topic);
            try
            {
                // Primero intentar verificar si el token ya está suscrito al tema
                // Desafortunadamente, Firebase no proporciona una API directa para verificar suscripciones
                // Por lo tanto, implementamos una lógica de manejo de errores robusta
                
                // Añadir mecanismo de reintento con timeout más corto
                int maxRetries = 3;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    _logger.LogInformation("Intento {Attempt} de {MaxRetries} para suscribir token al tópico {Topic}", 
                        attempt, maxRetries, topic);
                    try
                    {
                        await _firebaseMessaging.SubscribeToTopicAsync(new List<string> { token }, topic);
                        _logger.LogInformation("Token {Token} suscrito exitosamente al tópico {Topic}", token, topic);
                        return true;
                    }
                    catch (FirebaseMessagingException ex)
                    {
                        // Si el error indica que el token ya está suscrito (no hay un código específico, comprobamos mensaje)
                        if (ex.Message.Contains("already exists") || ex.Message.Contains("already subscribed"))
                        {
                            _logger.LogInformation("El token {Token} ya estaba suscrito al tópico {Topic}", token, topic);
                            return true;
                        }
                        else if (attempt < maxRetries && 
                                (ex.Message.Contains("timeout") || ex.Message.Contains("expire")))
                        {
                            // Si es un error de timeout o expiración y no es el último intento, esperar y reintentar
                            _logger.LogWarning(ex, "Error temporal (intento {Attempt}/{MaxRetries}) al suscribir token a tópico {Topic}. Esperando antes de reintentar...",
                                attempt, maxRetries, topic);
                            await Task.Delay(TimeSpan.FromSeconds(2 * attempt)); // Espera progresiva
                        }
                        else
                        {
                            throw; // Propagar otros errores para que se manejen en el catch externo
                        }
                    }
                    catch (Exception innerEx) when (attempt < maxRetries && 
                                                  (innerEx.Message.Contains("timeout") || 
                                                   innerEx.Message.Contains("expire")))
                    {
                        // Si es un error de timeout o expiración y no es el último intento, esperar y reintentar
                        _logger.LogWarning(innerEx, "Error temporal (intento {Attempt}/{MaxRetries}) al suscribir token a tópico {Topic}. Esperando antes de reintentar...", 
                            attempt, maxRetries, topic);
                        await Task.Delay(TimeSpan.FromSeconds(2 * attempt)); // Espera progresiva
                    }
                    catch (Exception innerEx)
                    {
                        // Si es otro tipo de error o es el último intento, registrarlo y lanzarlo
                        if (innerEx.Message.Contains("timeout") || innerEx.Message.Contains("expire"))
                        {
                            // Para errores de timeout o token expirado, intentar desuscribir y suscribir de nuevo
                            _logger.LogWarning(innerEx, "Error de timeout o token expirado. Intentando desuscribir primero y luego suscribir nuevamente");
                            try
                            {
                                // Intentar desuscribir primero si es posible (ignoramos errores aquí)
                                await UnsubscribeFromTopicPrivateAsync(token, topic, logErrors: false);
                                // Esperar un momento antes de intentar suscribir nuevamente
                                await Task.Delay(1000);
                                await _firebaseMessaging.SubscribeToTopicAsync(new List<string> { token }, topic);
                                _logger.LogInformation("Token {Token} suscrito exitosamente al tópico {Topic} después de desuscribir", token, topic);
                                return true;
                            }
                            catch (Exception retryEx)
                            {
                                _logger.LogError(retryEx, "Error al intentar el proceso de desuscripción/suscripción para el token");
                                throw;
                            }
                        }
                        else
                        {
                            _logger.LogError(innerEx, "Error no recuperable al suscribir token {Token} al tópico {Topic}", token, topic);
                            throw;
                        }
                    }
                }
                
                _logger.LogError("Se agotaron los intentos para suscribir el token {Token} al tópico {Topic}", token, topic);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al suscribir token {Token} al tópico {Topic}", token, topic);
                return false;
            }
        }

        public async Task<bool> SubscribeUserToTopicsAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || user.FcmTokens.Count == 0)
                    return false;

                // Subscribe all user tokens to appropriate topics
                foreach (var token in user.FcmTokens)
                {
                    await SubscribeToTopicAsync(token, "General");
                    
                    if (user.Role == UserRole.Representative)
                    {
                        await SubscribeToTopicAsync(token, "Representative");
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        await SubscribeToTopicAsync(token, "Admin");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user {UserId} to topics", userId);
                return false;
            }
        }

        public async Task<bool> UnsubscribeFromTopicAsync(string token, string topic)
        {
            return await UnsubscribeFromTopicPrivateAsync(token, topic, true);
        }

        private async Task<bool> UnsubscribeFromTopicPrivateAsync(string token, string topic, bool logErrors = true)
        {
            try
            {
                _logger.LogInformation("Intentando desuscribir el token {Token} del tópico {Topic}", token, topic);
                await _firebaseMessaging.UnsubscribeFromTopicAsync(new List<string> { token }, topic);
                _logger.LogInformation("Token {Token} desuscrito exitosamente del tópico {Topic}", token, topic);
                return true;
            }
            catch (Exception ex)
            {
                if (logErrors)
                {
                    _logger.LogError(ex, "Error al desuscribir token {Token} del tópico {Topic}", token, topic);
                }
                else
                {
                    _logger.LogWarning(ex, "Error al desuscribir token {Token} del tópico {Topic} (silenciado)", token, topic);
                }
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(string topic, string title, string body, object data = null)
        {
            try
            {
                var message = new Message
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Topic = topic
                };

                if (data != null)
                {
                    var dataDict = new Dictionary<string, string>();
                    foreach (var prop in data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(data)?.ToString();
                        if (value != null)
                        {
                            dataDict[prop.Name] = value;
                        }
                    }
                    message.Data = dataDict;
                }

                var response = await _firebaseMessaging.SendAsync(message);
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to topic {Topic}", topic);
                return false;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(string userId, string title, string body, object data = null)
        {
            _logger.LogInformation("Iniciando envío de notificación al usuario {UserId} con título: {Title}", userId, title);
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("No se encontró el usuario {UserId} para enviar la notificación", userId);
                    return false;
                }
                
                if (user.FcmTokens.Count == 0)
                {
                    _logger.LogWarning("El usuario {UserId} no tiene tokens FCM registrados", userId);
                    return false;
                }

                _logger.LogInformation("Usuario {UserId} tiene {TokenCount} tokens FCM registrados", userId, user.FcmTokens.Count);

                // Guardar el número original de tokens para comparar después
                int originalTokenCount = user.FcmTokens.Count;
                _logger.LogDebug("Número original de tokens: {OriginalTokenCount}", originalTokenCount);

                // Crear diccionario de datos si hay datos proporcionados
                Dictionary<string, string> dataDict = null;
                if (data != null)
                {
                    _logger.LogDebug("Procesando datos adicionales para la notificación");
                    dataDict = new Dictionary<string, string>();
                    foreach (var prop in data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(data)?.ToString();
                        if (value != null)
                        {
                            dataDict[prop.Name] = value;
                            _logger.LogDebug("Agregando dato: {Key}={Value}", prop.Name, value);
                        }
                    }
                }

                // Enviar mensajes individualmente a cada token en lugar de usar multicast
                int successCount = 0;
                var tokensToRemove = new List<string>(); // Lista para guardar tokens a eliminar
                
                _logger.LogInformation("Iniciando envío de notificaciones a {TokenCount} tokens", user.FcmTokens.Count);
                foreach (var token in user.FcmTokens)
                {
                    _logger.LogDebug("Intentando enviar notificación al token: {Token}", token);
                    try
                    {
                        var message = new Message
                        {
                            Token = token,
                            Notification = new FirebaseAdmin.Messaging.Notification
                            {
                                Title = title,
                                Body = body
                            },
                            Data = dataDict
                        };

                        // Enviar mensaje individual
                        _logger.LogDebug("Enviando mensaje a Firebase para token: {Token}", token);
                        var response = await _firebaseMessaging.SendAsync(message);
                        if (!string.IsNullOrEmpty(response))
                        {
                            successCount++;
                            _logger.LogDebug("Notificación enviada exitosamente al token {Token}, respuesta: {Response}", token, response);
                        }
                        else
                        {
                            _logger.LogWarning("Respuesta vacía al enviar notificación al token {Token}", token);
                        }
                    }
                    catch (FirebaseMessagingException ex)
                    {
                        // Si el token está desregistrado (UNREGISTERED), lo agregamos a la lista para eliminarlo después
                        if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                        {
                            _logger.LogWarning("Token desregistrado para el usuario {UserId}, marcando para eliminar: {Token}, código de error: {ErrorCode}", userId, token, ex.MessagingErrorCode);
                            tokensToRemove.Add(token);
                        }
                        else
                        {
                            _logger.LogError(ex, "Error enviando notificación al token {Token} del usuario {UserId}, código de error: {ErrorCode}", token, userId, ex.MessagingErrorCode);
                        }
                    }
                }

                _logger.LogInformation("Resultado del envío: {SuccessCount} notificaciones enviadas exitosamente de {TotalCount}", successCount, user.FcmTokens.Count);

                // Eliminar los tokens después de terminar la iteración
                if (tokensToRemove.Count > 0)
                {
                    _logger.LogInformation("Se encontraron {Count} tokens inválidos para eliminar", tokensToRemove.Count);
                    foreach (var tokenToRemove in tokensToRemove)
                    {
                        _logger.LogDebug("Eliminando token inválido: {Token}", tokenToRemove);
                        user.FcmTokens.Remove(tokenToRemove);
                    }
                    
                    _logger.LogInformation("Actualizando usuario {UserId} después de eliminar tokens inválidos", userId);
                    await _userRepository.UpdateAsync(user);
                    _logger.LogInformation("Se eliminaron {Count} tokens inválidos para el usuario {UserId}, tokens restantes: {RemainingTokens}", tokensToRemove.Count, userId, user.FcmTokens.Count);
                }

                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al enviar notificación al usuario {UserId}", userId);
                return false;
            }
        }
    }
}
