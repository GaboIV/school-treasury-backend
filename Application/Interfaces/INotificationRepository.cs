using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetAllAsync();
        Task<Notification> GetByIdAsync(string id);
        Task<List<Notification>> GetByTopicAsync(string topic);
        Task<List<Notification>> GetByUserIdAsync(string userId);
        Task<List<Notification>> GetScheduledNotificationsAsync();
        Task<List<Notification>> GetPendingNotificationsAsync();
        Task<string> CreateAsync(Notification notification);
        Task<bool> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(string id);
    }
} 