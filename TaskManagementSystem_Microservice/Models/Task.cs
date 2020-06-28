using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem_Microservice.Models
{
    /// <summary>
    /// Model class for Task Entity representing the Task table in the Microsoft SQL Server 2019 database
    /// If a Task doesn't contain SubTaskID (if SubTaskId is null), then it is the parent Task
    /// SubTasks would contain the same TaskID as its parent Task and its own SubTaskID
    /// </summary>
    public class Task
    {
        public int Id { get; set; }
        
        [Required]
        public short TaskId { get; set; }

        public short? SubTaskId { get; set; }
        
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime FinishDate { get; set; }

        public State State { get; set; }
    }

    public enum State { Completed, InProgress, Planned }
}
