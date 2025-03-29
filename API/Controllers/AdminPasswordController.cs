using System.Security.Claims;
using System.Threading.Tasks;
using API.Attributes;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/password")]
    [Authorize(Roles = "Administrator")]
    public class AdminPasswordController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AdminPasswordController> _logger;

        public AdminPasswordController(IAuthService authService, ILogger<AdminPasswordController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("change")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] AdminChangePasswordRequest request)
        {
            // Obtener el ID del administrador del token
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "Administrador no autenticado" });
            }

            var result = await _authService.AdminChangePasswordAsync(adminId, request);

            if (!result)
            {
                return BadRequest(new { message = "No se pudo cambiar la contraseña del usuario. Verifique que el usuario exista y que tenga permisos adecuados." });
            }

            return Ok(new { message = "Contraseña del usuario cambiada exitosamente" });
        }
    }
} 