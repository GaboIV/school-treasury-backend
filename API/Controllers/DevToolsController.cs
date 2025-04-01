using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Infrastructure.Persistence.Seeders;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SchoolTreasureAPI.Application.Interfaces;

namespace SchoolTreasureAPI.API.Controllers
{
    [Route("api/dev-tools")]
    [ApiController]
    public class DevToolsController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DevToolsController> _logger;

        public DevToolsController(
            IServiceProvider serviceProvider,
            IWebHostEnvironment environment,
            ILogger<DevToolsController> logger)
        {
            _serviceProvider = serviceProvider;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("run-seeders")]
        public async Task<IActionResult> RunSeeders()
        {
            // Solo permitir en entorno de desarrollo
            if (!_environment.IsDevelopment())
            {
                return Forbid("Esta operación solo está permitida en entorno de desarrollo.");
            }

            try
            {
                _logger.LogInformation("Iniciando seeders manualmente...");

                using var scope = _serviceProvider.CreateScope();
                var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                
                await databaseSeeder.SeedAllAsync();
                
                return Ok(new { message = "Seeders ejecutados con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar seeders manualmente");
                return StatusCode(500, $"Error al ejecutar seeders: {ex.Message}");
            }
        }

        [HttpPost("reset-app-versions")]
        public async Task<IActionResult> ResetAppVersions()
        {
            // Solo permitir en entorno de desarrollo
            if (!_environment.IsDevelopment())
            {
                return Forbid("Esta operación solo está permitida en entorno de desarrollo.");
            }

            try
            {
                _logger.LogInformation("Eliminando versiones de la aplicación...");

                using var scope = _serviceProvider.CreateScope();
                var appVersionService = scope.ServiceProvider.GetRequiredService<IAppVersionService>();
                var mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
                
                // Eliminar colección de versiones
                await mongoDatabase.DropCollectionAsync("AppVersions");
                _logger.LogInformation("Colección AppVersions eliminada");
                
                // Eliminar colección de info de la app
                await mongoDatabase.DropCollectionAsync("AppInfo");
                _logger.LogInformation("Colección AppInfo eliminada");
                
                // Ejecutar seeders nuevamente
                var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await databaseSeeder.SeedAllAsync();
                
                return Ok(new { message = "Datos de la aplicación reiniciados con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reiniciar datos de la aplicación");
                return StatusCode(500, $"Error al reiniciar datos de la aplicación: {ex.Message}");
            }
        }
    }
} 