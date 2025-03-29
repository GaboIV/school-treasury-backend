using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Notifications;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CustomNotificationService : ICustomNotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CustomNotificationService> _logger;

        public CustomNotificationService(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            ILogger<CustomNotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<NotificationDto>> GetAllAsync()
        {
            try
            {
                var notifications = await _notificationRepository.GetAllAsync();
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las notificaciones");
                throw;
            }
        }

        public async Task<NotificationDto> GetByIdAsync(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                return notification != null ? MapToDto(notification) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la notificación con ID {Id}", id);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetByTopicAsync(string topic)
        {
            try
            {
                var notifications = await _notificationRepository.GetByTopicAsync(topic);
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones por tema {Topic}", topic);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetPendingNotificationsAsync()
        {
            try
            {
                var notifications = await _notificationRepository.GetPendingNotificationsAsync();
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones pendientes");
                throw;
            }
        }

        public async Task<string> CreateAsync(CreateNotificationRequest request)
        {
            try
            {
                ValidateCreateRequest(request);
                
                var notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledFor = request.ScheduledFor,
                    IsSent = false,
                    Type = request.Type,
                    Topic = request.Topic,
                    TargetUserIds = request.TargetUserIds ?? new List<string>(),
                    AdditionalData = request.AdditionalData ?? new Dictionary<string, string>()
                };

                var id = await _notificationRepository.CreateAsync(notification);
                _logger.LogInformation("Notificación creada con ID {Id}", id);

                // Si no está programada, enviar inmediatamente
                if (!notification.ScheduledFor.HasValue)
                {
                    await SendNotificationAsync(id);
                }

                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación");
                throw;
            }
        }

        public async Task<NotificationDto> UpdateAsync(string id, UpdateNotificationRequest request)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    _logger.LogWarning("Notificación con ID {Id} no encontrada para actualizar", id);
                    return null;
                }

                if (notification.IsSent)
                {
                    throw new InvalidOperationException("No se puede actualizar una notificación que ya ha sido enviada");
                }

                // Actualizar solo los campos proporcionados
                if (request.Title != null)
                    notification.Title = request.Title;
                
                if (request.Body != null)
                    notification.Body = request.Body;
                
                if (request.ScheduledFor.HasValue)
                    notification.ScheduledFor = request.ScheduledFor;
                
                if (request.Type.HasValue)
                    notification.Type = request.Type.Value;
                
                if (request.Topic != null)
                    notification.Topic = request.Topic;
                
                if (request.TargetUserIds != null)
                    notification.TargetUserIds = request.TargetUserIds;
                
                if (request.AdditionalData != null)
                    notification.AdditionalData = request.AdditionalData;

                await _notificationRepository.UpdateAsync(notification);
                _logger.LogInformation("Notificación con ID {Id} actualizada", id);

                return MapToDto(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar notificación con ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    _logger.LogWarning("Notificación con ID {Id} no encontrada para eliminar", id);
                    return false;
                }

                if (notification.IsSent)
                {
                    throw new InvalidOperationException("No se puede eliminar una notificación que ya ha sido enviada");
                }

                var result = await _notificationRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Notificación con ID {Id} eliminada", id);
                }
                else
                {
                    _logger.LogWarning("Error al eliminar notificación con ID {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación con ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> SendNotificationAsync(string id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    _logger.LogWarning("Notificación con ID {Id} no encontrada para enviar", id);
                    return false;
                }

                if (notification.IsSent)
                {
                    _logger.LogWarning("Notificación con ID {Id} ya ha sido enviada", id);
                    return true;
                }

                bool result = false;

                // Preparar datos adicionales si existen
                object data = null;
                if (notification.AdditionalData != null && notification.AdditionalData.Count > 0)
                {
                    data = notification.AdditionalData;
                }

                // Enviar según el tipo de notificación
                switch (notification.Type)
                {
                    case NotificationType.TopicNotification:
                        if (string.IsNullOrEmpty(notification.Topic))
                        {
                            _logger.LogError("No se puede enviar una notificación por tema sin especificar el tema");
                            return false;
                        }
                        result = await _notificationService.SendNotificationAsync(
                            notification.Topic, notification.Title, notification.Body, data);
                        break;

                    case NotificationType.UserSpecificNotification:
                        if (notification.TargetUserIds == null || !notification.TargetUserIds.Any())
                        {
                            _logger.LogError("No se puede enviar una notificación específica sin destinatarios");
                            return false;
                        }

                        // Enviar a cada usuario específico
                        var userResults = new List<bool>();
                        foreach (var userId in notification.TargetUserIds)
                        {
                            var userResult = await _notificationService.SendNotificationToUserAsync(
                                userId, notification.Title, notification.Body, data);
                            userResults.Add(userResult);
                        }
                        // Consideramos éxito si al menos una notificación se envió correctamente
                        result = userResults.Any(r => r);
                        break;

                    case NotificationType.ScheduledNotification:
                        // Si es programada pero ya es hora de enviarla
                        if (notification.ScheduledFor.HasValue && notification.ScheduledFor.Value <= DateTime.UtcNow)
                        {
                            if (!string.IsNullOrEmpty(notification.Topic))
                            {
                                result = await _notificationService.SendNotificationAsync(
                                    notification.Topic, notification.Title, notification.Body, data);
                            }
                            else if (notification.TargetUserIds != null && notification.TargetUserIds.Any())
                            {
                                var specificUserResults = new List<bool>();
                                foreach (var userId in notification.TargetUserIds)
                                {
                                    var userResult = await _notificationService.SendNotificationToUserAsync(
                                        userId, notification.Title, notification.Body, data);
                                    specificUserResults.Add(userResult);
                                }
                                result = specificUserResults.Any(r => r);
                            }
                            else
                            {
                                _logger.LogError("Notificación programada sin tópico ni usuarios destinatarios");
                                return false;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Notificación programada para el futuro: {ScheduledFor}", notification.ScheduledFor);
                            return true; // No es error, simplemente aún no toca enviarla
                        }
                        break;
                }

                if (result)
                {
                    notification.IsSent = true;
                    await _notificationRepository.UpdateAsync(notification);
                    _logger.LogInformation("Notificación con ID {Id} enviada exitosamente", id);
                }
                else
                {
                    _logger.LogWarning("Error al enviar notificación con ID {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación con ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> ProcessScheduledNotificationsAsync()
        {
            try
            {
                var scheduledNotifications = await _notificationRepository.GetScheduledNotificationsAsync();
                var now = DateTime.UtcNow;
                
                var readyToSend = scheduledNotifications
                    .Where(n => !n.IsSent && n.ScheduledFor.HasValue && n.ScheduledFor.Value <= now)
                    .ToList();

                _logger.LogInformation("Procesando {Count} notificaciones programadas", readyToSend.Count);
                
                var results = new List<bool>();
                foreach (var notification in readyToSend)
                {
                    var result = await SendNotificationAsync(notification.Id);
                    results.Add(result);
                }

                return results.Any(r => r); // Éxito si al menos una notificación se envió
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar notificaciones programadas");
                return false;
            }
        }

        private void ValidateCreateRequest(CreateNotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("El título de la notificación es obligatorio");
            
            if (string.IsNullOrEmpty(request.Body))
                throw new ArgumentException("El cuerpo de la notificación es obligatorio");

            switch (request.Type)
            {
                case NotificationType.TopicNotification:
                    if (string.IsNullOrEmpty(request.Topic))
                        throw new ArgumentException("El tópico es obligatorio para notificaciones de tipo Topic");
                    break;
                
                case NotificationType.UserSpecificNotification:
                    if (request.TargetUserIds == null || !request.TargetUserIds.Any())
                        throw new ArgumentException("La lista de usuarios destinatarios es obligatoria para notificaciones específicas");
                    break;
                
                case NotificationType.ScheduledNotification:
                    if (!request.ScheduledFor.HasValue)
                        throw new ArgumentException("La fecha programada es obligatoria para notificaciones programadas");
                    
                    if (request.ScheduledFor < DateTime.UtcNow)
                        throw new ArgumentException("La fecha programada debe ser en el futuro");
                    
                    if (string.IsNullOrEmpty(request.Topic) && (request.TargetUserIds == null || !request.TargetUserIds.Any()))
                        throw new ArgumentException("Se debe especificar un tópico o lista de usuarios para notificaciones programadas");
                    break;
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                CreatedAt = notification.CreatedAt,
                ScheduledFor = notification.ScheduledFor,
                IsSent = notification.IsSent,
                Type = notification.Type,
                Topic = notification.Topic,
                TargetUserIds = notification.TargetUserIds,
                AdditionalData = notification.AdditionalData
            };
        }
    }
} 