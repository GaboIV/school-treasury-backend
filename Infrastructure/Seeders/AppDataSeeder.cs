using System;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using MongoDB.Driver;
using SchoolTreasureAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence.Seeders;
using System.IO;
using System.Collections.Generic;

namespace Infrastructure.Seeders
{
    public class AppDataSeeder : ISeeder
    {
        private readonly ILogger<AppDataSeeder> _logger;
        private readonly MongoDbContext _context;

        public AppDataSeeder(ILogger<AppDataSeeder> logger, MongoDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("=== INICIANDO SEEDER DE DATOS DE LA APLICACIÓN ===");

            try
            {
                await SeedAppInfoAsync();
                await SeedAppVersionsAsync();

                _logger.LogInformation("=== FINALIZADO SEEDER DE DATOS DE LA APLICACIÓN ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la siembra de datos de la aplicación");
                throw; // Propagar el error para que sea capturado por el DatabaseSeeder
            }
        }

        private async Task SeedAppInfoAsync()
        {
            try
            {
                _logger.LogInformation("Verificando si existe información de la aplicación...");
                
                // Verificar si ya existe información de la aplicación
                var appInfoCount = await _context.AppInfo.CountDocumentsAsync(_ => true);
                
                if (appInfoCount > 0)
                {
                    _logger.LogInformation("La información de la aplicación ya existe, omitiendo la siembra");
                    return;
                }

                _logger.LogInformation("Creando información inicial de la aplicación...");
                
                var appInfo = new AppInfo
                {
                    AppName = "School Treasury",
                    PackageName = "com.creativosla.schooltreasury",
                    Description = "Aplicación para gestionar la tesorería escolar de forma eficiente y transparente. Permite registrar colecciones, gastos, pagos de estudiantes y generar reportes.",
                    LogoUrl = "/static/images/logo.png",
                    Developer = "Creativos LA",
                    ContactEmail = "soporte@creativosla.com",
                    WebsiteUrl = "https://creativosla.com",
                    PrivacyPolicyUrl = "https://creativosla.com/privacy-policy",
                    LastUpdate = DateTime.UtcNow,
                    
                    Name = "School Treasury",
                    ImageUrl = "/static/images/app-logo.png",
                    Category = "Finanzas",
                    Version = "0.2.2",
                    Downloads = 1250,
                    Size = 12.5,
                    ReleaseDate = DateTime.UtcNow,
                    MinAndroidVersion = "5.0",
                    Rating = 5,
                    ChangeLog = new List<string>
                    {
                        "Sistema de actualización automática",
                        "Mejoras de seguridad",
                        "Corrección de errores reportados por usuarios",
                        "Optimización del consumo de batería",
                        "Nueva interfaz de tesorería",
                        "Soporte para múltiples monedas"
                    }
                };

                await _context.AppInfo.InsertOneAsync(appInfo);
                _logger.LogInformation("Información de la aplicación sembrada con éxito con ID: {Id}", appInfo.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sembrar información de la aplicación");
                throw;
            }
        }

        private async Task SeedAppVersionsAsync()
        {
            try
            {
                _logger.LogInformation("Verificando si existen versiones de la aplicación...");
                
                // Verificar si ya existen versiones
                var versionCount = await _context.AppVersions.CountDocumentsAsync(_ => true);
                
                if (versionCount > 0)
                {
                    _logger.LogInformation("Ya existen {Count} versiones de la aplicación, omitiendo la siembra", versionCount);
                    return;
                }

                _logger.LogInformation("Creando versiones iniciales de la aplicación...");
                
                // Asegurarnos de que existe el directorio apk
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apk");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    _logger.LogInformation("Creado directorio para APKs: {Directory}", directoryPath);
                }
                
                // Versión 0.1.0 - Primera versión beta
                var version010 = new AppVersion
                {
                    Version = "0.1.0",
                    ChangeLog = "Primera versión beta de la aplicación School Treasury:\n" +
                               "- Inicio de sesión para administradores\n" +
                               "- Gestión básica de estudiantes\n" +
                               "- Registro de pagos\n" +
                               "- Visualización de reportes simples",
                    ApkFilename = "update.v.0.1.0.apk",
                    IsAvailable = true,
                    IsRequired = true,
                    ReleaseDate = DateTime.UtcNow.AddDays(-90)
                };
                
                // Versión 0.1.1 - Correcciones de errores
                var version011 = new AppVersion
                {
                    Version = "0.1.1",
                    ChangeLog = "Corrección de errores:\n" +
                               "- Solucionado problema con inicio de sesión\n" +
                               "- Mejorada la estabilidad general\n" +
                               "- Corregido error al guardar pagos",
                    ApkFilename = "update.v.0.1.1.apk",
                    IsAvailable = true,
                    IsRequired = true,
                    ReleaseDate = DateTime.UtcNow.AddDays(-60)
                };
                
                // Versión 0.2.0 - Nuevas funcionalidades
                var version020 = new AppVersion
                {
                    Version = "0.2.0",
                    ChangeLog = "Nuevas funcionalidades:\n" +
                               "- Módulo de gastos\n" +
                               "- Gestión de tipos de colección\n" +
                               "- Reportes avanzados\n" +
                               "- Notificaciones push\n" +
                               "- Mejoras en la interfaz de usuario",
                    ApkFilename = "update.v.0.2.0.apk",
                    IsAvailable = true,
                    IsRequired = false,
                    ReleaseDate = DateTime.UtcNow.AddDays(-30)
                };
                
                // Versión 0.2.1 - Mejoras menores
                var version021 = new AppVersion
                {
                    Version = "0.2.1",
                    ChangeLog = "Mejoras menores:\n" +
                               "- Optimización de rendimiento\n" +
                               "- Mejoras en la interfaz de usuario\n" +
                               "- Soporte para dispositivos más antiguos",
                    ApkFilename = "update.v.0.2.1.apk",
                    IsAvailable = true,
                    IsRequired = false,
                    ReleaseDate = DateTime.UtcNow.AddDays(-15)
                };
                
                // Versión actual 0.2.2
                var version022 = new AppVersion
                {
                    Version = "0.2.2",
                    ChangeLog = "Versión actual:\n" +
                               "- Sistema de actualización automática\n" +
                               "- Mejoras de seguridad\n" +
                               "- Corrección de errores reportados por usuarios\n" +
                               "- Optimización del consumo de batería",
                    ApkFilename = "update.v.0.2.2.apk",
                    IsAvailable = true,
                    IsRequired = false,
                    ReleaseDate = DateTime.UtcNow
                };

                // Crear archivos APK de prueba
                var versions = new[] 
                { 
                    version010, 
                    version011, 
                    version020, 
                    version021, 
                    version022 
                };

                foreach (var version in versions)
                {
                    var filePath = Path.Combine(directoryPath, version.ApkFilename);
                    if (!File.Exists(filePath))
                    {
                        // Crear un archivo APK de prueba
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            byte[] content = System.Text.Encoding.UTF8.GetBytes($"This is a fake APK file for version {version.Version}");
                            await fileStream.WriteAsync(content, 0, content.Length);
                        }
                        _logger.LogInformation("Creado archivo APK de prueba: {FilePath}", filePath);
                    }
                }

                // Insertar todas las versiones en la base de datos
                await _context.AppVersions.InsertManyAsync(versions);
                _logger.LogInformation("Se sembraron {Count} versiones de la aplicación con éxito", versions.Length);
                
                foreach (var version in versions)
                {
                    _logger.LogInformation("Sembrada versión {Version}", version.Version);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sembrar versiones de la aplicación");
                throw;
            }
        }
    }
} 