using System;

namespace ToDoListApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        // Add a new property to track if a notification has been sent
        public bool IsNotified { get; set; } = false;
    }
}