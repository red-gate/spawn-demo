using NSubstitute;
using NUnit.Framework;
using Spawn.Demo.Store;
using Spawn.Demo.WebApi.Controllers;
using Spawn.Demo.WebApi.Tests.Spawnctl;
using Serilog;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Linq;
using NUnit.Framework.Interfaces;

namespace Spawn.Demo.WebApi.Tests
{
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    [Parallelizable(ParallelScope.All)]
    public class TodoApiTests
    {
        private const string TestUserId = "test@example.com";
        private readonly SpawnClient _spawnClient;
        private TodoController _todoController;
        private string _todoDataContainer = null;

        public TodoApiTests()
        {
            _spawnClient = new SpawnClient(TestContext.Out);
        }

        [SetUp]
        public void Setup()
        {
            _todoDataContainer = _spawnClient.CreateDataContainer(SetupFixture.TodoDataImageName);

            var todoConnString = _spawnClient.GetConnectionString(_todoDataContainer, SpawnClient.EngineType.Postgres);
            var logger = Substitute.For<ILogger>();
            var todoConnectionService = new TodoConnectionService(todoConnString, true);
            var todoStore = new TodoStore(todoConnectionService, logger);
            var projectStore = new ProjectStore(todoConnectionService, logger);

            _todoController = new TodoController(todoStore, projectStore, logger);
            _todoController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = TestHelpers.GetUser(TestUserId) }
            };
        }

        [TearDown]
        public void Teardown()
        {
            if(TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
              _spawnClient.CreateImageFromCurrentContainerState(_todoDataContainer, $"todo-{TestContext.CurrentContext.Test.ID}", "--tag", SetupFixture.TodoDataImageTag, "--team", "red-gate:sharks");
            }
            // Don't wait for these tasks to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataContainer(_todoDataContainer));
        }

        [Test]
        public async Task WhenInvokingAddTodo_ThenOneTodoIsAdded()
        {
            var result = await _todoController.GetAsync();
            var todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Empty);

            await _todoController.RecordAsync(new Models.TodoItem
            {
                CreatedAt = DateTime.Now,
                Done = false,
                Task = "My new todo task",
            });

            result = await _todoController.GetAsync();
            todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty);
            Assert.That(todos.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenInvokingDeleteTodo_ThenOneTodoIsDeleted()
        {
            await _todoController.RecordAsync(new Models.TodoItem
            {
                CreatedAt = DateTime.Now,
                Done = false,
                Task = "My new todo task",
            });

            var result = await _todoController.GetAsync();
            var todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty);
            Assert.That(todos.Count(), Is.EqualTo(1));

            var foundTodoItem = todos.ElementAt(0);

            await _todoController.DeleteAsync(foundTodoItem);

            result = await _todoController.GetAsync();
            todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Empty);
        }

        [Test]
        public async Task WhenInvokingUpdateTodo_ThenOneTodoIsUpdated()
        {
            await _todoController.RecordAsync(new Models.TodoItem
            {
                CreatedAt = DateTime.Now,
                Done = false,
                Task = "My new todo task",
            });

            var result = await _todoController.GetAsync();
            var todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty);
            Assert.That(todos.Count(), Is.EqualTo(1));

            var foundTodoItem = todos.ElementAt(0);

            const string updatedTodoTask = "New updated todo task";
            foundTodoItem.Task = updatedTodoTask;

            await _todoController.PutAsync(foundTodoItem);

            result = await _todoController.GetAsync();
            todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty);

            var updatedTodoItem = todos.ElementAt(0);
            Assert.That(updatedTodoItem.Task, Is.EqualTo(updatedTodoTask));
        }

        [Test]
        public async Task WhenDeletingOneTodoItem_ThenOnlyOneTodoItemIsDeleted()
        {
            await _todoController.RecordAsync(new Models.TodoItem
            {
                CreatedAt = DateTime.Now,
                Done = false,
                Task = "My first todo task",
            });
            await _todoController.RecordAsync(new Models.TodoItem
            {
                CreatedAt = DateTime.Now,
                Done = false,
                Task = "My second todo task",
            });

            var result = await _todoController.GetAsync();
            var todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty);
            Assert.That(todos.Count(), Is.EqualTo(2));

            var firstTodoItem = todos.ElementAt(0);

            await _todoController.DeleteAsync(firstTodoItem);

            result = await _todoController.GetAsync();
            todos = TestHelpers.GetObjectResultContent<IEnumerable<Models.TodoItem>>(result);
            Assert.That(todos, Is.Not.Empty, message: "Expected 1 remaining todo item after deleting, but todos was actually empty");
            Assert.That(todos.Count(), Is.EqualTo(1));
            Assert.That(todos.Any(x => x.Task == "My second todo task"), Is.True);
        }
    }

}