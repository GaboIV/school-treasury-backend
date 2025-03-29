using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Notifications;

namespace Application.Interfaces
{
    public interface ICustomNotificationService
    {
        Task<List<NotificationDto>> GetAllAsync();
        Task<NotificationDto> GetByIdAsync(string id);
        Task<List<NotificationDto>> GetByTopicAsync(string topic);
        Task<List<NotificationDto>> GetByUserIdAsync(string userId);
        Task<List<NotificationDto>> GetPendingNotificationsAsync();
        Task<string> CreateAsync(CreateNotificationRequest request);
        Task<NotificationDto> UpdateAsync(string id, UpdateNotificationRequest request);
        Task<bool> DeleteAsync(string id);
        Task<bool> SendNotificationAsync(string id);
        Task<bool> ProcessScheduledNotificationsAsync();
    }
} 