using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(IMongoDatabase database, ILogger<NotificationRepository> logger)
        {
            _notifications = database.GetCollection<Notification>("Notifications");
            _logger = logger;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            try
            {
                return await _notifications.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las notificaciones");
                throw;
            }
        }

        public async Task<Notification> GetByIdAsync(string id)
        {
            try
            {
                return await _notifications.Find(n => n.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificación con ID {Id}", id);
                throw;
            }
        }

        public async Task<List<Notification>> GetByTopicAsync(string topic)
        {
            try
            {
                return await _notifications.Find(n => n.Topic == topic).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones por tópico {Topic}", topic);
                throw;
            }
        }

        public async Task<List<Notification>> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _notifications.Find(n => n.TargetUserIds.Contains(userId)).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Notification>> GetScheduledNotificationsAsync()
        {
            try
            {
                return await _notifications.Find(n => n.ScheduledFor != null).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones programadas");
                throw;
            }
        }

        public async Task<List<Notification>> GetPendingNotificationsAsync()
        {
            try
            {
                return await _notifications.Find(n => !n.IsSent).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones pendientes");
                throw;
            }
        }

        public async Task<string> CreateAsync(Notification notification)
        {
            try
            {
                await _notifications.InsertOneAsync(notification);
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Notification notification)
        {
            try
            {
                var result = await _notifications.ReplaceOneAsync(n => n.Id == notification.Id, notification);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar notificación con ID {Id}", notification.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _notifications.DeleteOneAsync(n => n.Id == id);
                return result.IsAcknowledged && result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar notificación con ID {Id}", id);
                throw;
            }
        }
    }
} 