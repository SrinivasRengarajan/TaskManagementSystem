using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using TaskManagementSystem_Microservice.DBContexts;
using TaskManagementSystem_Microservice.Models;

/// <summary>
/// Repository Design Pattern serving all the CRUD API Calls as well as the CSV ReportGeneration API call
/// Contains Business Logics
/// Repository is a component of a micro service encapsulating data access layer which persists the data 
/// </summary>

namespace TaskManagementSystem_Microservice.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskContext _dbContext;

        /// <summary>
        /// Dependency Injection of DbContext
        /// </summary>
        /// <param name="dbContext"></param>
        public TaskRepository(TaskContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Service Methods
        public dynamic DeleteTask(int taskId)
        {
            //Retrieving all the tasks and subtasks with a particular task id.
            var tasks = _dbContext.Tasks.Where(p => p.TaskId == taskId).ToList();

            //If tasks found, then delete it
            if (tasks.Any())
            {
                //Deleting those tasks and its subtasks from the table in the database
                _dbContext.Tasks.RemoveRange(tasks);

                //Saving changes made through this Save method
                Save();

                return "Success";
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Task> GetTaskByID(int taskId)
        {
            //To get the task and its subtasks with the supplied taskID
            return _dbContext.Tasks.Where(p=>p.TaskId == taskId).ToList();
        }

        public IEnumerable<Task> GetTasks()
        {
            //Get the list of all the tasks from the table in the database using database context
            return _dbContext.Tasks.ToList();
        }

        public dynamic ExportCSV(DateTime dateTime)
        {
            //To retrieve all the tasks or subtasks in progress matching the given StartDate
            var progresses = _dbContext.Tasks.Where(p => p.StartDate == dateTime && p.State == State.InProgress).ToList();

            //If Tasks exists, write them to memory stream in csv format 
            if (progresses.Count() > 0)
            {

                //Creating a memory stream object for writing the records into it
                var stream = new MemoryStream();

                using (var writeFile = new StreamWriter(stream, leaveOpen: true))
                {
                    //CsvHelper Package is downloaded from the Nugget Package Manager and used for exporting the report in csv format
                    var csv = new CsvWriter(writeFile, new System.Globalization.CultureInfo("en-US"));

                    //Writing the records in a csv format into the memory stream
                    csv.WriteRecords(progresses);
                }
                stream.Position = 0; //resetting the stream

                return stream;
            }

            else
            {
                return "No Tasks Available for the Selected Date!";
            }
        }

        public dynamic InsertTask(Task task)
        {
            List<Task> allTasks = _dbContext.Tasks.AsNoTracking().ToList();

            //If a Task has a valid SubTaskId, then it is a SubTask of some parent Task
            if (task.SubTaskId != null)
            {
                //To check if the SubTask has a valid parent Task
                var isParentTaskAvailable = allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId == null).Count()==1;
                
                if (isParentTaskAvailable)
                {
                    //To check if the SubTask of a parent Task is not repeating or redundant 
                    var isSubTaskRedundant= allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId == task.SubTaskId).Any();

                    //If the SubTask is not redundant and valid, adding it to the Tasks table in the database
                    if (!isSubTaskRedundant)
                    {
                        _dbContext.Add(task);
                        Save();
                        return "Success";
                    }
                    return null;
                }
                return null;
            }

            //If the SubTaskId is null, then it is a parent Task
            else
            {
                //To check if any Parent Tasks exists in the table with the same TaskId, ensuring only one parent Task can be there for a Task
                var isParentTaskRedundant = allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId == null).Any();
                
                //If it is a new Task, then adding it to the Tasks table in the database
                if (!isParentTaskRedundant)
                {
                    _dbContext.Add(task);
                    Save();
                    return "Success";
                }
                return null;
            }
        }

        public dynamic UpdateTask(Task task)
        {
            var allTasks = _dbContext.Tasks.AsNoTracking().ToList();

            //Retrieving the Task to be updated from the database matching the TaskId and SubTaskId provided by the user.
            var taskExists = allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId == task.SubTaskId).FirstOrDefault();

            //If the Task to be updated is a parent Task
            if(taskExists!=null && taskExists.SubTaskId==null)
            {
                task.Id = taskExists.Id;

                //Modifying the entry for Parent Task
                _dbContext.Entry(task).State = EntityState.Modified;

                //Saving the changes
                Save();

                return "Updated Successfully";
            }

            //If the Task to be updated is a SubTask
            else if (taskExists != null && taskExists.SubTaskId != null)
            {
                var parentTask = allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId == null).FirstOrDefault();

                //Setting the Id of the Task to be updated with its matching Task's Id to enable change tracking 
                task.Id = taskExists.Id;

                allTasks.Where(p => p.Id == task.Id).ToList().ForEach(s=>s.State=task.State);

                //Retrieving all the SubTasks of a particular Task
                var subTasks = allTasks.Where(p => p.TaskId == task.TaskId && p.SubTaskId != null).ToList();

                //If SubTasks are available
                if (subTasks.Any())
                {
                    //If any of those SubTasks is in InProgress state, then updating its parent task to InProgress state
                    if (subTasks.Any(x => x.State == State.InProgress))
                    {
                        parentTask.State = State.InProgress;
                    }

                    //If all of those SubTasks are in Completed state, then updating its parent task to Completed state
                    else if (subTasks.All(x => x.State == State.Completed))
                    {
                        parentTask.State = State.Completed;
                    }

                    //Otherwise updating its parent task to Planned state
                    else
                    {
                        parentTask.State = State.Completed;
                    }
                }

                //Modifying the entry for Parent Task
                _dbContext.Entry(parentTask).State = EntityState.Modified;

                //Modifying the entry for SubTask
                _dbContext.Entry(task).State = EntityState.Modified;

                //Saving the changes
                Save();

                return "Updated Successfully";
            }

            else
            {
                return null;
            }
        }

        #endregion

        #region Helper Method
        public void Save()
        {
            //Saving all the changes made to the Tasks table in the database
            _dbContext.SaveChanges();
        }

        #endregion
    }
}