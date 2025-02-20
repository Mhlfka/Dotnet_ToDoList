using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoListApp.Data;
using ToDoListApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ToDoListApp.Services
{
    public class NotificationService
    {
        private readonly ToDoContext _context;

        public NotificationService(ToDoContext context)
        {
            _context = context;
        }

        // Method to fetch tasks that are due today and need notifications
        public async Task<List<TaskItem>> GetDueTasksForNotificationAsync()
        {
            var dueTasks = await _context.TaskItems
                .Where(t => t.DueDate.Date == DateTime.Today && !t.IsNotified && !t.IsCompleted)
                .ToListAsync();

            return dueTasks;
        }

        // This method is for marking tasks as notified (can be called from the controller after the notification is shown)
        public async Task MarkTasksAsNotifiedAsync(List<TaskItem> tasks)
        {
            foreach (var task in tasks)
            {
                task.IsNotified = true; // Mark as notified
            }

            await _context.SaveChangesAsync(); // Save notification status updates
        }
    }
}
