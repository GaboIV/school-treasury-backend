using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.API.Controllers
{
    [Route("api/app-info")]
    [ApiController]
    public class AppInfoController : ControllerBase
    {
        private readonly IAppInfoService _appInfoService;
        private readonly ILogger<AppInfoController> _logger;

        public AppInfoController(IAppInfoService appInfoService, ILogger<AppInfoController> logger)
        {
            _appInfoService = appInfoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppInfo()
        {
            try
            {
                var appInfo = await _appInfoService.GetAppInfoAsync();
                
                if (appInfo == null)
                {
                    return NotFound("Información de la aplicación no encontrada.");
                }
                
                return Ok(appInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de la aplicación");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAppInfo([FromBody] AppInfo appInfo)
        {
            try
            {
                if (appInfo == null)
                {
                    return BadRequest("Datos de la aplicación no válidos.");
                }

                // Obtener la información actual
                var currentAppInfo = await _appInfoService.GetAppInfoAsync();
                
                if (currentAppInfo == null || string.IsNullOrEmpty(currentAppInfo.Id))
                {
                    return NotFound("No existe información de la aplicación para actualizar.");
                }

                // Asegurar que se mantenga el mismo ID
                appInfo.Id = currentAppInfo.Id;
                appInfo.LastUpdate = DateTime.UtcNow;
                
                var updatedAppInfo = await _appInfoService.UpdateAppInfoAsync(appInfo);
                return Ok(updatedAppInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar información de la aplicación");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppInfo([FromBody] AppInfo appInfo)
        {
            try
            {
                if (appInfo == null)
                {
                    return BadRequest("Datos de la aplicación no válidos.");
                }

                // Verificar si ya existe información
                var currentAppInfo = await _appInfoService.GetAppInfoAsync();
                
                if (currentAppInfo != null)
                {
                    return BadRequest("Ya existe información de la aplicación. Use el método PUT para actualizarla.");
                }

                appInfo.LastUpdate = DateTime.UtcNow;
                var success = await _appInfoService.CreateAppInfoAsync(appInfo);
                
                if (!success)
                {
                    return BadRequest("No se pudo crear la información de la aplicación.");
                }
                
                return CreatedAtAction(nameof(GetAppInfo), null, appInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear información de la aplicación");
                return StatusCode(500, "Error interno del servidor.");
            }
        }
    }
} 