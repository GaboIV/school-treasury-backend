using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class FcmTokenRequest
    {
        [Required(ErrorMessage = "El token FCM es obligatorio")]
        public string Token { get; set; }
    }
}
