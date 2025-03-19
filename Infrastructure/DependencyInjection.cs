using Application.Interfaces;
using Application.Services;
using Infrastructure.Logging;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Seeders;
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

            // Registrar el LoggerManager
            services.AddSingleton<ILoggerManager, LoggerManager>();

            // Registrar repositorios
            services.AddScoped<ICollectionRepository, CollectionRepository>();
            services.AddScoped<ICollectionTypeRepository, CollectionTypeRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IStudentPaymentRepository, StudentPaymentRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IPettyCashRepository, PettyCashRepository>();
            services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IInterestLinkRepository, InterestLinkRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Registrar seeders
            services.AddScoped<ISeeder, StudentSeeder>();
            services.AddScoped<DatabaseSeeder>();
            services.AddScoped<UserSeeder>();

            // Registrar servicios
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<ICollectionTypeService, CollectionTypeService>();
            services.AddScoped<IStudentPaymentService, StudentPaymentService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IPettyCashService, PettyCashService>();
            services.AddScoped<ITransactionLogService, TransactionLogService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IInterestLinkService, InterestLinkService>();

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
