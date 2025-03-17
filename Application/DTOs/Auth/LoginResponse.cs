using Domain.Enums;

namespace Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public string Token { get; set; }
        public string StudentId { get; set; }
    }
} 