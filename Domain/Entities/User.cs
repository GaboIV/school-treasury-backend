using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public string StudentId { get; set; } // Referencia al estudiante si el usuario es un Representante
        public bool IsActive { get; set; } = true;
        public DateTime LastLogin { get; set; }
        public bool HasChangedPassword { get; set; } = false;
        public List<string> FcmTokens { get; set; } = new List<string>();
        
        public User()
        {
            // Constructor vacío para MongoDB
            FcmTokens = new List<string>();
        }
        
        public User(string username, string passwordHash, string email, string fullName, UserRole role)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            FullName = fullName;
            Role = role;
            LastLogin = DateTime.UtcNow;
            HasChangedPassword = false;
            FcmTokens = new List<string>();
        }
    }
}