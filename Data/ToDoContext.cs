using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Models;

namespace ToDoListApp.Data
{
    public class ToDoContext : IdentityDbContext<IdentityUser>
    {
        public ToDoContext(DbContextOptions<ToDoContext> options)
            : base(options) { }

        public DbSet<TaskItem> TaskItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.DueDate); // Indexing DueDate for better query performance

            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.IsCompleted); // Indexing IsCompleted for efficient filtering
        }
    }
}
