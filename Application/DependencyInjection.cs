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
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IExpenseTypeService, ExpenseTypeService>();
            services.AddScoped<IStudentPaymentService, StudentPaymentService>();

            // Identificar mappings automáticamente
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            // ExpenseType
            services.AddScoped<IExpenseTypeRepository, ExpenseTypeRepository>();
            services.AddScoped<IExpenseTypeService, ExpenseTypeService>();

            // Expense
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IExpenseService, ExpenseService>();

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
