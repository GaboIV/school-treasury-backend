using System.Security.Claims;
using System.Threading.Tasks;
using API.Attributes;
using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IAuthService _authService;

        public ChangePasswordController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Obtener el ID del usuario del token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result)
            {
                return BadRequest(new { message = "No se pudo cambiar la contraseña. Verifique que la contraseña actual sea correcta." });
            }

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
    }
}
