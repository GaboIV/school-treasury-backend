using Application.Interfaces;
using Application.Services;
using Infrastructure.Logging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Seeders;
using MongoDB.Driver;
using Gabonet.Hubble.Extensions;

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

            // Configuración básica para Hubble
            services.AddHttpContextAccessor();
            services.AddHubble(options =>
            {
                options.ConnectionString = configuration["MongoDB:ConnectionString"];
                options.DatabaseName = configuration["MongoDB:DatabaseName"];
                options.ServiceName = configuration["Hubble:ServiceName"] ?? "MyAppService";
                options.TimeZoneId = configuration["Hubble:TimeZoneId"];
                options.CaptureLoggerMessages = true;
                options.MinimumLogLevel = LogLevel.Information;
            });
            
            // Registrar MongoDbContext
            services.AddSingleton<MongoDbContext>(provider => {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                return new MongoDbContext(connectionString, databaseName, httpContextAccessor);
            });
            
            services.AddSingleton<IMongoClient>(sp => {
                var mongoDbContext = sp.GetRequiredService<MongoDbContext>();
                return mongoDbContext.GetMongoClient();
            });
            
            services.AddSingleton<IMongoDatabase>(sp => {
                var mongoDbContext = sp.GetRequiredService<MongoDbContext>();
                return mongoDbContext.GetMongoDatabase();
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
