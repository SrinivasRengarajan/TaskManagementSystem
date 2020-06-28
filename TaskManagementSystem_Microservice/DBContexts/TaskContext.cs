using Microsoft.EntityFrameworkCore;
using System;
using TaskManagementSystem_Microservice.Models;

namespace TaskManagementSystem_Microservice.DBContexts
{
    /// <summary>
    /// Database Context which acts as a middleware to communicate with the database. 
    /// It contains DbSet properties which has the database table's data
    /// </summary>
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for storing Tasks data
        /// </summary>
        public DbSet<Task> Tasks { get; set; }

        /// <summary>
        /// Overriding OnModelCreating method to convert the property value to and from the database. (enum into string and back)
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Task>()
                .Property(p => p.State)
                .HasConversion(
                    v => v.ToString(),
                    v => (State)Enum.Parse(typeof(State), v));
        }

    }
}
