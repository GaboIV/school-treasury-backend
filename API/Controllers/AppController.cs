using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolTreasureAPI.Application.DTOs;
using SchoolTreasureAPI.Application.Interfaces;
using SchoolTreasureAPI.Domain.Entities;

namespace SchoolTreasureAPI.API.Controllers
{
    [Route("api/apps")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly IAppInfoService _appInfoService;
        private readonly IAppVersionService _appVersionService;
        private readonly IDownloadStatService _downloadStatService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AppController> _logger;

        public AppController(
            IAppInfoService appInfoService,
            IAppVersionService appVersionService,
            IDownloadStatService downloadStatService,
            IWebHostEnvironment environment,
            ILogger<AppController> logger)
        {
            _appInfoService = appInfoService;
            _appVersionService = appVersionService;
            _downloadStatService = downloadStatService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("main")]
        public async Task<IActionResult> GetAppInfo()
        {
            try
            {
                var appInfo = await _appInfoService.GetAppInfoAsync();

                if (appInfo == null)
                {
                    return NotFound("Información de la aplicación no encontrada.");
                }

                // Actualizar la información dinámica
                var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();
                if (latestVersion != null && appInfo.LatestVersion != null)
                {
                    // Actualizar la versión actual en la información de la app
                    appInfo.Version = latestVersion.Version;
                    appInfo.ReleaseDate = latestVersion.ReleaseDate;

                    // Aseguramos que se incluya el changelog en el formato correcto
                    // if (!string.IsNullOrEmpty(latestVersion.ChangeLog) && latestVersion.ChangeLog.Contains('\n'))
                    // {
                    //     appInfo.ChangeLog = latestVersion.ChangeLog.Split('\n').ToList();
                    // }
                }

                // Obtener el conteo total de descargas para mostrarlo en el frontend
                var downloadStats = await _downloadStatService.GetStatsAsync();
                if (downloadStats != null)
                {
                    appInfo.Downloads = downloadStats.TotalDownloads;
                }

                return Ok(appInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de la aplicación");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions()
        {
            try
            {
                var versions = await _appVersionService.GetAllVersionsAsync();
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener versiones de la aplicación");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("check-update")]
        public async Task<IActionResult> CheckForUpdate([FromQuery] string currentVersion)
        {
            try
            {
                if (string.IsNullOrEmpty(currentVersion))
                {
                    return BadRequest("La versión actual debe ser especificada.");
                }

                // Obtener la URL base para construir la URL de descarga
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                var updateInfo = await _appInfoService.CheckForUpdateAsync(currentVersion, baseUrl);
                return Ok(updateInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar actualizaciones");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("download/latest")]
        public async Task<IActionResult> DownloadLatest([FromQuery] DownloadStatDTO statDTO)
        {
            try
            {
                var latestVersion = await _appVersionService.GetLatestAvailableVersionAsync();

                if (latestVersion == null)
                {
                    return NotFound("No hay versiones disponibles para descargar.");
                }

                // Registrar la estadística de descarga
                if (statDTO != null)
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _downloadStatService.RecordDownloadAsync(
                        latestVersion.Id,
                        latestVersion.Version,
                        statDTO,
                        ipAddress
                    );
                }

                var apkFilePath = Path.Combine(_environment.WebRootPath, "apk", latestVersion.ApkFilename);

                if (!System.IO.File.Exists(apkFilePath))
                {
                    return NotFound("Archivo APK no encontrado.");
                }

                _logger.LogInformation("Sirviendo archivo APK: {Filename}", latestVersion.ApkFilename);
                var fileStream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, "application/vnd.android.package-archive", latestVersion.ApkFilename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar la última versión");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadVersion(string id, [FromQuery] DownloadStatDTO statDTO)
        {
            try
            {
                var version = await _appVersionService.GetVersionByIdAsync(id);

                if (version == null)
                {
                    return NotFound($"Versión con ID {id} no encontrada.");
                }

                // Registrar la estadística de descarga
                if (statDTO != null)
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _downloadStatService.RecordDownloadAsync(
                        version.Id,
                        version.Version,
                        statDTO,
                        ipAddress
                    );
                }

                var apkFilePath = Path.Combine(_environment.WebRootPath, "apk", version.ApkFilename);

                if (!System.IO.File.Exists(apkFilePath))
                {
                    return NotFound("Archivo APK no encontrado.");
                }

                _logger.LogInformation("Sirviendo archivo APK específico: {Filename}", version.ApkFilename);
                var fileStream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, "application/vnd.android.package-archive", version.ApkFilename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar versión específica");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("download/stats")]
        public async Task<IActionResult> RecordDownloadStat([FromBody] DownloadStatDTO statDTO, [FromQuery] string versionId, [FromQuery] string version)
        {
            try
            {
                if (string.IsNullOrEmpty(versionId) || string.IsNullOrEmpty(version))
                {
                    return BadRequest("Se requiere ID de versión y número de versión.");
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var stat = await _downloadStatService.RecordDownloadAsync(
                    versionId,
                    version,
                    statDTO,
                    ipAddress
                );

                return Ok(new { Message = "Estadística de descarga registrada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar estadística de descarga");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string version)
        {
            try
            {
                StatsResponseDTO stats;

                if (startDate.HasValue && endDate.HasValue)
                {
                    stats = await _downloadStatService.GetStatsByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else if (!string.IsNullOrEmpty(version))
                {
                    stats = await _downloadStatService.GetStatsByVersionAsync(version);
                }
                else
                {
                    stats = await _downloadStatService.GetStatsAsync();
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, "Error interno del servidor.");
            }
        }
    }
}