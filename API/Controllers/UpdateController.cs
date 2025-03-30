using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace SchoolTreasureAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IAppVersionService _appVersionService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UpdateController> _logger;

        public UpdateController(IAppVersionService appVersionService, IWebHostEnvironment environment, ILogger<UpdateController> logger)
        {
            _appVersionService = appVersionService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("lastest-apk")]
        public async Task<IActionResult> GetLatestApk()
        {
            var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();
            
            if (latestVersion == null)
            {
                return NotFound("No hay versiones disponibles para descargar.");
            }

            var apkFilePath = Path.Combine(_environment.WebRootPath, "apk", latestVersion.ApkFilename);
            
            if (!System.IO.File.Exists(apkFilePath))
            {
                return NotFound("Archivo APK no encontrado.");
            }

            var fileStream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, "application/vnd.android.package-archive", latestVersion.ApkFilename);
        }

        [HttpGet("check-update")]
        public async Task<IActionResult> CheckForUpdate([FromQuery] string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion))
            {
                return BadRequest("La versión actual debe ser especificada.");
            }

            var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();
            
            if (latestVersion == null)
            {
                return NotFound("No hay versiones disponibles.");
            }

            var isUpdateAvailable = await _appVersionService.IsUpdateAvailableAsync(currentVersion);
            
            return Ok(new 
            {
                IsUpdateAvailable = isUpdateAvailable,
                LatestVersion = latestVersion.Version,
                ChangeLog = latestVersion.ChangeLog,
                ReleaseDate = latestVersion.ReleaseDate,
                IsRequired = latestVersion.IsRequired
            });
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions()
        {
            var versions = await _appVersionService.GetAllVersionsAsync();
            return Ok(versions);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100MB
        [RequestSizeLimit(104857600)] // 100MB
        public async Task<IActionResult> UploadApk([FromForm] AppVersionUploadDTO versionDTO)
        {
            try
            {
                _logger.LogInformation("Iniciando carga de APK con versión: {Version}", versionDTO.Version);
                
                if (versionDTO.ApkFile == null || versionDTO.ApkFile.Length == 0)
                {
                    _logger.LogWarning("Se intentó cargar un APK vacío o nulo");
                    return BadRequest("No se ha proporcionado un archivo APK válido");
                }

                _logger.LogInformation("Tamaño del archivo APK: {Size} bytes", versionDTO.ApkFile.Length);

                if (string.IsNullOrEmpty(versionDTO.Version))
                {
                    return BadRequest("La versión es requerida");
                }

                // Verificar formato de la versión (debe ser x.x.x)
                if (!IsValidVersionFormat(versionDTO.Version))
                {
                    return BadRequest("El formato de la versión debe ser x.x.x (ejemplo: 1.0.0)");
                }

                // Generamos el nombre del archivo APK
                string apkFileName = $"update.v.{versionDTO.Version}.apk";
                string apkDirectory = Path.Combine(_environment.WebRootPath, "apk");

                // Crear el directorio si no existe
                if (!Directory.Exists(apkDirectory))
                {
                    Directory.CreateDirectory(apkDirectory);
                    _logger.LogInformation("Se creó el directorio para APKs: {Directory}", apkDirectory);
                }

                string filePath = Path.Combine(apkDirectory, apkFileName);
                _logger.LogInformation("Guardando APK en: {FilePath}", filePath);

                // Guardar el archivo APK
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await versionDTO.ApkFile.CopyToAsync(stream);
                }

                _logger.LogInformation("Archivo APK guardado exitosamente");

                // Crear el registro de versión en la base de datos
                var appVersion = new AppVersion
                {
                    Version = versionDTO.Version,
                    ChangeLog = versionDTO.ChangeLog,
                    ApkFilename = apkFileName,
                    IsAvailable = versionDTO.IsAvailable,
                    IsRequired = versionDTO.IsRequired,
                    ReleaseDate = DateTime.UtcNow
                };

                var savedVersion = await _appVersionService.AddVersionAsync(appVersion);
                _logger.LogInformation("Versión guardada en la base de datos con ID: {Id}", savedVersion.Id);

                return Ok(new
                {
                    Id = savedVersion.Id,
                    Version = savedVersion.Version,
                    ChangeLog = savedVersion.ChangeLog,
                    ApkFilename = savedVersion.ApkFilename,
                    IsAvailable = savedVersion.IsAvailable,
                    IsRequired = savedVersion.IsRequired,
                    ReleaseDate = savedVersion.ReleaseDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el archivo APK");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("version/{id}")]
        public async Task<IActionResult> UpdateVersion(string id, [FromBody] AppVersionDTO versionDTO)
        {
            try
            {
                _logger.LogInformation("Actualizando versión con ID: {Id}", id);
                
                var existingVersion = await _appVersionService.GetVersionByIdAsync(id);
                
                if (existingVersion == null)
                {
                    _logger.LogWarning("No se encontró versión con ID: {Id}", id);
                    return NotFound($"No se encontró la versión con ID {id}");
                }

                // Actualizar solo los campos permitidos
                existingVersion.ChangeLog = versionDTO.ChangeLog;
                existingVersion.IsAvailable = versionDTO.IsAvailable;
                existingVersion.IsRequired = versionDTO.IsRequired;

                var updatedVersion = await _appVersionService.UpdateVersionAsync(existingVersion);
                _logger.LogInformation("Versión actualizada exitosamente: {Id}", id);

                return Ok(new
                {
                    Id = updatedVersion.Id,
                    Version = updatedVersion.Version,
                    ChangeLog = updatedVersion.ChangeLog,
                    ApkFilename = updatedVersion.ApkFilename,
                    IsAvailable = updatedVersion.IsAvailable,
                    IsRequired = updatedVersion.IsRequired,
                    ReleaseDate = updatedVersion.ReleaseDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la versión con ID: {Id}", id);
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("version/{id}")]
        public async Task<IActionResult> DeleteVersion(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando versión con ID: {Id}", id);
                
                var existingVersion = await _appVersionService.GetVersionByIdAsync(id);
                
                if (existingVersion == null)
                {
                    _logger.LogWarning("No se encontró versión con ID: {Id}", id);
                    return NotFound($"No se encontró la versión con ID {id}");
                }

                // Eliminar el archivo APK
                string apkFilePath = Path.Combine(_environment.WebRootPath, "apk", existingVersion.ApkFilename);
                if (System.IO.File.Exists(apkFilePath))
                {
                    System.IO.File.Delete(apkFilePath);
                    _logger.LogInformation("Archivo APK eliminado: {Path}", apkFilePath);
                }

                // Eliminar el registro de la base de datos
                await _appVersionService.DeleteVersionAsync(id);
                _logger.LogInformation("Versión eliminada exitosamente de la base de datos: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la versión con ID: {Id}", id);
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private bool IsValidVersionFormat(string version)
        {
            if (string.IsNullOrEmpty(version))
                return false;
                
            return version.Split('.').All(part => int.TryParse(part, out _));
        }
    }
}
