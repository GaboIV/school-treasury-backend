using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs
{
    public class ProcessScheduledNotificationsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessScheduledNotificationsJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public ProcessScheduledNotificationsJob(
            IServiceProvider serviceProvider,
            ILogger<ProcessScheduledNotificationsJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de procesamiento de notificaciones programadas iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar notificaciones programadas");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessNotificationsAsync()
        {
            _logger.LogInformation("Iniciando procesamiento de notificaciones programadas");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<ICustomNotificationService>();
                var result = await notificationService.ProcessScheduledNotificationsAsync();
                
                if (result)
                {
                    _logger.LogInformation("Notificaciones programadas procesadas exitosamente");
                }
                else
                {
                    _logger.LogInformation("No había notificaciones programadas para procesar o hubo errores");
                }
            }
        }
    }
} 