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

            return services;
        }
    }
}
