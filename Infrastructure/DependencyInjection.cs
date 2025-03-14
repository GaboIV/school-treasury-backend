using Application.Interfaces;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MongoDB.Driver;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is missing.");

            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<ICollectionRepository, CollectionRepository>();
            services.AddScoped<ICollectionTypeRepository, CollectionTypeRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IStudentPaymentRepository, StudentPaymentRepository>();

            // Registrar seeders
            services.AddScoped<ISeeder, StudentSeeder>();
            services.AddScoped<DatabaseSeeder>();

            // Registrar servicios
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPettyCashRepository, PettyCashRepository>();
            services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();

            return services;
        }

        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await databaseSeeder.SeedAllAsync();
        }
    }
}
