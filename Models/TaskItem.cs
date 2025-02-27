using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.DateTime)] // Ensure it handles both date & time
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsNotified { get; set; } = false;
    }

}
