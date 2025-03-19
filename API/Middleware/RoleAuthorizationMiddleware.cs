using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Rutas públicas que no requieren autenticación
            var publicRoutes = new[]
            {
                "/api/v1/auth/login",
                "/swagger",
                "/health"
            };

            // Verificar si la ruta actual es pública
            var currentPath = context.Request.Path.Value?.ToLower() ?? string.Empty;
            var isPublicRoute = publicRoutes.Any(route => currentPath.StartsWith(route.ToLower()));

            if (isPublicRoute)
            {
                await _next(context);
                return;
            }

            // Verificar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                _logger.LogInformation($"Usuario autenticado: {userId}, Rol: {userRole}, Ruta: {currentPath}");
                
                // Rutas que requieren rol de Administrador
                var adminRoutes = new[]
                {
                    "/api/v1/auth/register",
                    // Agregar otras rutas que requieran rol de administrador
                };

                // Verificar si la ruta actual requiere rol de Administrador
                var requiresAdmin = adminRoutes.Any(route => currentPath.StartsWith(route.ToLower()));

                if (requiresAdmin && userRole != "Administrator")
                {
                    _logger.LogWarning($"Acceso denegado para usuario {context.User.Identity.Name} a ruta {currentPath}. Rol requerido: Administrator, Rol actual: {userRole}");
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "No tienes permisos para acceder a este recurso" });
                    return;
                }
            }
            else
            {
                _logger.LogInformation($"Usuario no autenticado intentando acceder a: {currentPath}");
            }

            await _next(context);
        }
    }
} 