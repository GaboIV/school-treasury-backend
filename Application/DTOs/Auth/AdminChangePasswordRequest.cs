using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class AdminChangePasswordRequest
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public string UserId { get; set; }
        
        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(3, ErrorMessage = "La contraseña debe tener al menos 3 caracteres")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
} 