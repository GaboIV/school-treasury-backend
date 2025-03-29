using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public bool IsSent { get; set; }
        public NotificationType Type { get; set; }
        public string Topic { get; set; }
        public List<string> TargetUserIds { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }
} 