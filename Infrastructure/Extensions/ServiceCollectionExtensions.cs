using System;
using System.Threading.Tasks;
using Infrastructure.Seeders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<UserSeeder>>();

            try
            {
                logger.LogInformation("Iniciando seeders...");
                
                var userSeeder = services.GetRequiredService<UserSeeder>();
                await userSeeder.SeedAsync();
                
                logger.LogInformation("Seeders completados exitosamente.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar seeders.");
            }
        }
    }
} 