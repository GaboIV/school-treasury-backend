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
                    var credentialJson = configuration["Firebase:CredentialJson"];
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
                return false;

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                _logger.LogInformation("User: {User}", user);
                if (user == null)
                    return false;

                _logger.LogInformation("FCM token: {FcmToken}", fcmToken);
                // Add token if it doesn't exist
                if (!user.FcmTokens.Contains(fcmToken))
                {
                    _logger.LogInformation("Adding FCM token to user {UserId}", userId);
                    user.FcmTokens.Add(fcmToken);
                    await _userRepository.UpdateAsync(user);

                    // Subscribe to topics based on user role
                    await SubscribeToTopicAsync(fcmToken, "General");
                    _logger.LogInformation("Subscribed to General topic");
                    
                    if (user.Role == UserRole.Representative)
                    {
                        await SubscribeToTopicAsync(fcmToken, "Representative");
                        _logger.LogInformation("Subscribed to Representative topic");
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        await SubscribeToTopicAsync(fcmToken, "Admin");
                        _logger.LogInformation("Subscribed to Admin topic");
                    }
                }

                _logger.LogInformation("FCM token added to user {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding FCM token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveTokenAsync(string userId, string fcmToken)
        {
            if (string.IsNullOrEmpty(fcmToken))
                return false;

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                // Remove token if it exists
                if (user.FcmTokens.Contains(fcmToken))
                {
                    user.FcmTokens.Remove(fcmToken);
                    await _userRepository.UpdateAsync(user);

                    // Unsubscribe from topics
                    await UnsubscribeFromTopicAsync(fcmToken, "General");
                    
                    if (user.Role == UserRole.Representative)
                    {
                        await UnsubscribeFromTopicAsync(fcmToken, "Representative");
                    }
                    else if (user.Role == UserRole.Administrator)
                    {
                        await UnsubscribeFromTopicAsync(fcmToken, "Admin");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing FCM token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SubscribeToTopicAsync(string token, string topic)
        {
            try
            {
                await _firebaseMessaging.SubscribeToTopicAsync(new List<string> { token }, topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing token to topic {Topic}", topic);
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
            try
            {
                await _firebaseMessaging.UnsubscribeFromTopicAsync(new List<string> { token }, topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing token from topic {Topic}", topic);
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(string topic, string title, string body, object data = null)
        {
            try
            {
                var message = new Message
                {
                    Notification = new Notification
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
                            Notification = new Notification
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
