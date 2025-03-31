using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders
{
    public class DatabaseSeeder
    {
        private readonly IEnumerable<ISeeder> _seeders;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(IEnumerable<ISeeder> seeders, ILogger<DatabaseSeeder> logger)
        {
            _seeders = seeders;
            _logger = logger;
        }

        public async Task SeedAllAsync()
        {
            _logger.LogInformation("======= INICIANDO SEEDERS DE BASE DE DATOS =======");
            
            try
            {
                var seedersCount = 0;
                foreach (var seeder in _seeders)
                {
                    _logger.LogInformation("Ejecutando seeder: {SeederType}", seeder.GetType().Name);
                    
                    try
                    {
                        await seeder.SeedAsync();
                        seedersCount++;
                        _logger.LogInformation("Seeder completado con éxito: {SeederType}", seeder.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error ejecutando seeder {SeederType}", seeder.GetType().Name);
                        throw; // Propagar el error para detener el proceso
                    }
                }
                
                _logger.LogInformation("======= SEEDERS COMPLETADOS: {Count} =======", seedersCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general durante la ejecución de seeders");
                throw;
            }
        }
    }
} 