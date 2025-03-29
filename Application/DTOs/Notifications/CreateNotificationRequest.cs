using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Notifications
{
    public class CreateNotificationRequest
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "El cuerpo es obligatorio")]
        public string Body { get; set; }
        
        public DateTime? ScheduledFor { get; set; }
        
        [Required(ErrorMessage = "El tipo de notificación es obligatorio")]
        public NotificationType Type { get; set; }
        
        // Si es de tipo TopicNotification, se requiere el topic
        public string? Topic { get; set; }
        
        // Si es de tipo UserSpecificNotification, se requiere lista de IDs de usuarios
        public List<string>? TargetUserIds { get; set; }
        
        // Datos adicionales opcionales
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
} 