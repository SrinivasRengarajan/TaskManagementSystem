using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using TaskManagementSystem_Microservice.Controllers;
using TaskManagementSystem_Microservice.Models;
using TaskManagementSystem_Microservice.Repository;
using Xunit;

/// <summary>
/// Unit Tests for testing the controller actions
/// </summary>
namespace Web_Api_Unit_Tests.Controller
{
    public class TaskWebApiControllerTest
    {
        private readonly Mock<ITaskRepository> _mockRepo;
        private readonly TaskController _controller;

        public TaskWebApiControllerTest()
        {
            //Instantiating a mock reporsitory object
            _mockRepo = new Mock<ITaskRepository>();
            
            //Instantiating a controller object
            _controller = new TaskController(_mockRepo.Object);
        }

        #region Test Cases

        [Fact]
        public void Get_WhenCalled_ReturnsOkResult()
        {
            // Arrange
            _mockRepo.Setup(p => p.GetTasks()).Returns(new List<Task>() { new Task() { Id = 5, TaskId = 150, SubTaskId = 1, Name = "Test Task" } });

            //Act
            var okResult = _controller.Get();

            // Assert
            Assert.IsType<OkObjectResult>(okResult);

        }

        [Fact]
        public void Get_WhenCalled_ReturnsAllItems()
        {
            // Arrange
            _mockRepo.Setup(p => p.GetTasks()).Returns(new List<Task>() { new Task() { Id = 5, TaskId = 150, SubTaskId = 1, Name = "Test Task" } });

            //Act
            var okResult = _controller.Get() as OkObjectResult;

            // Assert 
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void GetByTaskId_UnknownTaskIdPassed_ReturnsNotFoundResult()
        {
            // Arrange
            _mockRepo.Setup(p => p.GetTaskByID(It.IsAny<int>())).Returns(new List<Task>());

            // Act
            var notFoundResult = _controller.Get(140);

            // Assert
            Assert.IsType<NotFoundObjectResult>(notFoundResult);
        }

        [Fact]
        public void GetByTaskId_ExistingTaskIdPassed_ReturnsOkResult()
        {
            // Arrange
            _mockRepo.Setup(p => p.GetTaskByID(It.IsAny<int>())).Returns(new List<Task>() { new Task() { Id = 5, TaskId = 150, SubTaskId = 1, Name = "Test Task" } });
            int taskId=150;

            // Act
            var okResult = _controller.Get(taskId);

            // Assert
            Assert.IsType<OkObjectResult>(okResult);
        }

        [Fact]
        public void GetByTaskId_ExistingTaskIdPassed_ReturnsValidData()
        {
            // Arrange
            _mockRepo.Setup(p => p.GetTaskByID(It.IsAny<int>())).Returns(new List<Task>() { new Task() { Id = 5, TaskId = 150, SubTaskId = 1, Name = "Test Task" } });
            var taskId = 150;

            // Act
            var okResult = _controller.Get(taskId) as OkObjectResult;

            // Assert
            Assert.IsType<List<Task>>(okResult.Value);
        }

        [Fact]
        public void Insert_InvalidObjectPassed_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var badResponse = _controller.Post(task:null);

            // Assert
            Assert.IsType<BadRequestResult>(badResponse);
        }


        [Fact]
        public void Insert_ValidObjectPassed_ReturnsCreatedResponse()
        {
            // Arrange
            _mockRepo.Setup(p => p.InsertTask(It.IsAny<Task>())).Returns(new CreatedAtActionResult("Post","Task",600,new Task()));

            // Act
            var result = _controller.Post(new Task { TaskId = 600 });

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public void Update_InvalidObjectPassed_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var badResponse = _controller.Put(task: null);

            // Assert
            Assert.IsType<BadRequestResult>(badResponse);
        }


        [Fact]
        public void Update_ValidObjectPassed_ReturnsOkayResult()
        {
            // Arrange
            _mockRepo.Setup(p => p.UpdateTask(It.IsAny<Task>())).Returns(new OkObjectResult("Updated Successfully"));

            // Act
            var result = _controller.Put(new Task { TaskId = 600 });

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetReport_NullorEmptyDatePassed_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(p => p.ExportCSV(It.IsAny<DateTime>())).Returns(new NotFoundObjectResult("Invalid Input"));

            // Act
            var result = _controller.GetReport("");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetReport_ValidDatePassed_ReturnsFile()
        {
            // Arrange
            _mockRepo.Setup(p => p.ExportCSV(It.IsAny<DateTime>())).Returns(new MemoryStream());

            // Act
            var result = _controller.GetReport("2020/07/16");

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public void GetReport_InvalidDatePassed_ReturnsMessage()
        {
            // Arrange
            _mockRepo.Setup(p => p.ExportCSV(It.IsAny<DateTime>())).Returns(new string("No Tasks Available for the Selected Date!"));

            // Act
            var result = _controller.GetReport("2020/07/13");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Delete_InvalidTaskIdPassed_ReturnsBadRequest()
        {
            // Act
            _controller.Delete(500);

            // Assert
            _mockRepo.Verify(v => v.DeleteTask(500), Times.Once());
        }

        [Fact]
        public void Delete_ValidTaskIdPassed_ReturnsOkResult()
        {
            // Arrange
            _mockRepo.Setup(p => p.DeleteTask(It.IsAny<int>())).Returns(new OkObjectResult("Success"));
            var taskId = 500;

            // Act
            var okResponse = _controller.Delete(taskId);

            // Assert
            Assert.IsType<OkObjectResult>(okResponse);
        }

        #endregion
    }
}
