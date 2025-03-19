using Domain.Enums;
using API.Attributes;
using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            
            if (response == null)
            {
                return Unauthorized(new { message = "Nombre de usuario o contraseña incorrectos" });
            }

            return Ok(response);
        }

        [HttpPost("register")]
        [AuthorizeRoles(UserRole.Administrator)] // Solo los administradores pueden registrar nuevos usuarios
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            
            if (!result)
            {
                return BadRequest(new { message = "El nombre de usuario o correo electrónico ya está en uso" });
            }

            return Ok(new { message = "Usuario registrado exitosamente" });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            Console.WriteLine("Entrando al servicios");
            return Ok(new
            {
                Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                FullName = User.FindFirst("FullName")?.Value
            });
        }
    }
}
