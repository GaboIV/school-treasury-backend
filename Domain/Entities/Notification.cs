using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public bool IsSent { get; set; }
        public NotificationType Type { get; set; }
        public string Topic { get; set; }  // General, Admin, Representative
        public List<string> TargetUserIds { get; set; }  // Para envío a usuarios específicos
        public Dictionary<string, string> AdditionalData { get; set; }

        public Notification()
        {
            CreatedAt = DateTime.UtcNow;
            IsSent = false;
            TargetUserIds = new List<string>();
            AdditionalData = new Dictionary<string, string>();
        }
    }
} 