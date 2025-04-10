using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Data;
using ToDoListApp.Models;

namespace ToDoListApp.Services
{
    public class NotificationService
    {
        private readonly ToDoContext _context;
        private readonly YandexEmailSender _emailSender;

        public NotificationService(ToDoContext context, YandexEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // ✅ Method used by the background service
        public async Task<List<TaskItem>> GetOverdueTasksAsync()
        {
            var now = DateTime.Now;
            return await _context.TaskItems
                .Where(t => !t.IsCompleted && t.DueDate < now)
                .ToListAsync();
        }

        // ✅ Existing method (you already had this)
        public async Task NotifyOverdueTasksAsync()
        {
            var now = DateTime.Now;
            var overdueTasks = await _context.TaskItems
                .Where(t => !t.IsCompleted && t.DueDate < now && !t.IsNotified)
                .ToListAsync();

            if (overdueTasks.Any())
            {
                string subject = "📌 Overdue Task Notification";
                string body = "The following tasks are overdue:\n\n" +
                    string.Join("\n", overdueTasks.Select(t => $"- {t.Description} (Due: {t.DueDate})"));

                await _emailSender.SendEmailAsync("abdugafforkhon@yandex.com", subject, body); // Using async email method

                // Optionally mark them as notified:
                foreach (var task in overdueTasks)
                {
                    task.IsNotified = true;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
