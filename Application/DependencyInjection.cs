using System.Reflection;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar servicios de la capa de Application
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IExpenseTypeService, ExpenseTypeService>();

            // Identificar mappings automáticamente
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
