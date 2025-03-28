using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task<bool> AddTokenAsync(string userId, string fcmToken);
        Task<bool> RemoveTokenAsync(string userId, string fcmToken);
        Task<bool> SubscribeToTopicAsync(string token, string topic);
        Task<bool> SubscribeUserToTopicsAsync(string userId);
        Task<bool> UnsubscribeFromTopicAsync(string token, string topic);
        Task<bool> SendNotificationAsync(string topic, string title, string body, object data = null);
        Task<bool> SendNotificationToUserAsync(string userId, string title, string body, object data = null);
    }
}
