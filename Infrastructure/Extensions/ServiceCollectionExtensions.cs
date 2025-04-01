using System;
using System.Threading.Tasks;
using Infrastructure.Seeders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence.Seeders;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeding");

            try
            {
                logger.LogInformation("Iniciando proceso de sembrado de datos...");
                
                // Utilizar el DatabaseSeeder para ejecutar todos los seeders registrados
                var databaseSeeder = services.GetRequiredService<DatabaseSeeder>();
                await databaseSeeder.SeedAllAsync();
                
                logger.LogInformation("Proceso de sembrado de datos completado exitosamente.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante el proceso de sembrado de datos.");
            }
        }
    }
} 