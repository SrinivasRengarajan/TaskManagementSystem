using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Transactions;
using TaskManagementSystem_Microservice.ExceptionHandling;
using TaskManagementSystem_Microservice.Models;
using TaskManagementSystem_Microservice.Repository;

/// <summary>
/// Application: Task Management System MicroService
/// Developer Name: Srinivas Rengarajan
/// Date: 26/06/2020
/// </summary>

namespace TaskManagementSystem_Microservice.Controllers
{
    /// <summary>
    /// ASP .Net Core Web API Controller - EndPoint for MicroService exposing the http methods to the clients as endpoints for the   service methods
    /// </summary>

    //This ApiController attribute .net core takes care of the model validation errors by default by returning Bad Request
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        //Repository Interface Object
        private readonly ITaskRepository _taskRepository;

        /// <summary>
        /// Dependency Injection of Repository Interface
        /// </summary>
        /// <param name="taskRepository"></param>
        public TaskController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        #region API Calls

        // GET: api/Task
        [HttpGet]
        public IActionResult Get()
        {
            //To get all the tasks and subtasks available in the database
            var tasks = _taskRepository.GetTasks();

            //If no tasks found
            if (tasks.Count()==0)
            {
                return NotFound(new ApiResponse(404, "Data Not Available"));
            }

            //If tasks found
            return Ok(tasks);
        }

        // GET api/Task/2
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            //To get the task and its subtasks by task id
            var task = _taskRepository.GetTaskByID(id);

            //If no matching task found
            if (task.Count() == 0)
            {
                return NotFound(new ApiResponse(404, $"Task not found with id {id}"));
            }

            //If task found
            return Ok(task);
        }

        [Route("[action]/{date}")]
        [HttpGet]
        public IActionResult GetReport(string date)
        {
            //To generate a csv report file for all the tasks in progress for a selected date
            if (!string.IsNullOrEmpty(date))
            {
                DateTime dateValue;

                //To check whether the date string is valid and if valid convert the date string to a valid datetime object
                if(!DateTime.TryParse(date, out dateValue))
                {
                    return null;
                }

                //Calling the ExportCSV method in the repository
                var stream = _taskRepository.ExportCSV(dateValue);

                if(stream.GetType()==typeof(MemoryStream))
                {
                    //To download the report file in csv format
                    return File(stream, "application/octet-stream", "Reports.csv");
                }
                else
                {
                    //If no Tasks exists for the selected date, then return a custom message back to the user
                    return Ok(stream);
                }
            }
            else
            {
                return NotFound(new ApiResponse(204));
            }
        }

        /// <summary>
        /// Insert a task if it is a valid object, else returns Bad Request (taken care by [ApiController] attribute)
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post(Task task)
        {
            //TransactionScope is used for executing the db commands as a block availing the commit and rollback options.
            using (var scope = new TransactionScope())
            {
                //Calling InsertTask repository method for creating a new task or subtask
                var result=_taskRepository.InsertTask(task);
                //The Complete method is used to commit the transaction, if an exception is thrown in the above method, complete
                //will not be called and the transaction will be rolled back.
                scope.Complete();

                //If Operation not success
                if (result == null)
                {
                    return BadRequest();
                }
                else
                {
                    return CreatedAtAction(nameof(Get), new { id = task.TaskId }, task);
                }
            }
        }

        /// <summary>
        /// Update the task if it is a valid object, else returns Bad Request (taken care by [ApiController] attribute)
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPut]
        public IActionResult Put(Task task)
        {
            using (var scope = new TransactionScope())
            {
               //Calling UpdateTask repository method for modifying/updating an existing task or subtask
               var result =_taskRepository.UpdateTask(task);
               scope.Complete();

               if(result==null)
               {
                    return BadRequest();
               }
               else
               {
                    return Ok(result);
               }
            }
        }

        // DELETE api/Task/2
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            //Calling the DeleteTask method in the repository to delete the parent task with its subtasks
            var result=_taskRepository.DeleteTask(id);
            if(result==null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(result);
            }
        }

        
        
        [Route("error/{code}")]
        public IActionResult Error(int code)
        {
            //Error Handling Object containing Api Response messages in the page
            return new ObjectResult(new ApiResponse(code));
        }

        #endregion
    }
}
