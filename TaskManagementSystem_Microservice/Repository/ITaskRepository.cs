using System;
using System.Collections.Generic;
using System.IO;
using TaskManagementSystem_Microservice.Models;

namespace TaskManagementSystem_Microservice.Repository
{
    /// <summary>
    /// Interface for Repository
    /// Containing API CRUD calls for Tasks and call for Report Generation
    /// </summary>
    public interface ITaskRepository
    {
        #region Method Signatures

        IEnumerable<Task> GetTasks();
        IEnumerable<Task> GetTaskByID(int taskId);
        dynamic InsertTask(Task task);
        dynamic DeleteTask(int taskId);
        dynamic UpdateTask(Task task);
        dynamic ExportCSV(DateTime dateTime);
        void Save();

        #endregion
    }
}
