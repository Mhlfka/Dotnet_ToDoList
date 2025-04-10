using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoListApp.Services
{
    public class OverdueTaskNotification : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OverdueTaskNotification> _logger;

        public OverdueTaskNotification(IServiceProvider services, ILogger<OverdueTaskNotification> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OverdueTaskNotification starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                        var overdueTasks = await notificationService.GetOverdueTasksAsync();

                        if (overdueTasks.Any())
                        {
                            string subject = "📌 Overdue Tasks Reminder";
                            string body = "The following tasks are overdue:\n\n" +
                                          string.Join("\n", overdueTasks.Select(t =>
                                              $"- {t.Description} (Due: {t.DueDate})"));

                            var emailSender = scope.ServiceProvider.GetRequiredService<YandexEmailSender>();
                            await emailSender.SendEmailAsync("abdugafforkhon@yandex.com", subject, body);
                        }
                        else
                        {
                            _logger.LogInformation("No overdue tasks found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing overdue tasks");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

            _logger.LogInformation("OverdueTaskNotification stopped.");
        }
    }
}   