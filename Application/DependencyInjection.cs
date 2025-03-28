using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar servicios de la capa de Application
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<ICollectionTypeService, CollectionTypeService>();
            services.AddScoped<IStudentPaymentService, StudentPaymentService>();
            
            // Registrar ExpenseService después de PettyCashService para asegurar la dependencia
            services.AddScoped<IPettyCashService, PettyCashService>();
            services.AddScoped<IExpenseService, ExpenseService>();

            // Identificar mappings automáticamente
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            // CollectionType
            services.AddScoped<ICollectionTypeRepository, CollectionTypeRepository>();
            services.AddScoped<ICollectionTypeService, CollectionTypeService>();

            // Collection
            services.AddScoped<ICollectionRepository, CollectionRepository>();
            services.AddScoped<ICollectionService, CollectionService>();

            // Student
            services.AddScoped<IStudentRepository, StudentRepository>();

            // StudentPayment
            services.AddScoped<IStudentPaymentRepository, StudentPaymentRepository>();
            services.AddScoped<IStudentPaymentService, StudentPaymentService>();

            // PettyCash y Logs
            services.AddScoped<IPettyCashService, PettyCashService>();
            services.AddScoped<ITransactionLogService, TransactionLogService>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Dashboard
            services.AddScoped<IDashboardService, DashboardService>();

            // InterestLink
            services.AddScoped<IInterestLinkService, InterestLinkService>();
            services.AddScoped<IInterestLinkRepository, InterestLinkRepository>();

            return services;
        }
    }
}
