using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ToDoListApp.Data;
using ToDoListApp.Models;
using ToDoListApp.Services;

namespace ToDoListApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly ToDoContext _context;
        private readonly NotificationService _notificationService;

        public TaskController(ToDoContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch all tasks to display in the view
            var tasks = await _context.TaskItems.ToListAsync();

            // Get tasks due today that need notification
            var dueTasks = await _notificationService.GetDueTasksForNotificationAsync();

            // Pass the due tasks to the view (for displaying notifications)
            ViewData["DueTasks"] = dueTasks;

            // Mark tasks as notified (this should be done after the notification)
            await _notificationService.MarkTasksAsNotifiedAsync(dueTasks);

            return View("Index", tasks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem newTask)
        {
            if (ModelState.IsValid)
            {
                _context.TaskItems.Add(newTask);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(newTask);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return View(taskItem);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(updatedTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(updatedTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(updatedTask);
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task != null)
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
