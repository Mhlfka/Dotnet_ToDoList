using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index(string? description, DateTime? dueDate, bool? isCompleted)
        {
            var filteredTasks = _context.TaskItems.AsQueryable();

            if (!string.IsNullOrEmpty(description))
            {
                filteredTasks = filteredTasks.Where(t => t.Description.Contains(description));
            }
            if (dueDate.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.DueDate.Date == dueDate.Value.Date); // Match only the date part
            }
            if (isCompleted.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.IsCompleted == isCompleted.Value);
            }

            var taskList = await filteredTasks.ToListAsync();

            ViewData["NoTasksFound"] = taskList.Any() ? null : "No such task found. Showing all tasks.";
            return View("~/Views/Home/Index.cshtml", taskList.Any() ? taskList : await _context.TaskItems.ToListAsync());
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem newTask)
        {
            if (ModelState.IsValid)
            {
                bool taskExists = await _context.TaskItems
                    .AnyAsync(t => t.DueDate == newTask.DueDate);

                if (taskExists)
                {
                    ViewData["DuplicateTask"] = "A task with this Due Date & Time already exists!";
                    return View(newTask);
                }

                _context.TaskItems.Add(newTask);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(newTask);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return View(taskItem);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool taskExists = await _context.TaskItems
                    .AnyAsync(t => t.DueDate == updatedTask.DueDate && t.Id != id);

                if (taskExists)
                {
                    ViewData["DuplicateTask"] = "A task with this Due Date & Time already exists!";
                    return View(updatedTask);
                }

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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [Authorize(Roles = "Admin")]
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

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isCompleted)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.IsCompleted = isCompleted;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to update status." });
            }
        }
    }
}
