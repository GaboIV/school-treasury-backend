using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "El rol es obligatorio")]
        public UserRole Role { get; set; }
        
        // Solo requerido si el rol es Representative
        public string StudentId { get; set; }
    }
} 