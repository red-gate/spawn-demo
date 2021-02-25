using NSubstitute;
using NUnit.Framework;
using Spawn.Demo.WebApi.Tests.Spawnctl;
using Spawn.Demo.WebApi.Controllers;
using Spawn.Demo.Store;
using Serilog;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using NUnit.Framework.Interfaces;

namespace Spawn.Demo.WebApi.Tests
{
    [TestFixture]
    [Category("Smoke")]
    public class SmokeTests
    {
        private const string SmokeTestUserId = "rtslikbz.rjntjthub@hqdevt.org";
        private const string SmokeTestDataImage = "demo-todo:large";
        private readonly SpawnClient _spawnClient;
        private TodoController _todoController;
        private string _todoDataContainer = null;

        public SmokeTests()
        {
            _spawnClient = new SpawnClient(TestContext.Progress);
        }

        [SetUp]
        public void Setup()
        {
            _todoDataContainer = _spawnClient.CreateDataContainer(SmokeTestDataImage);

            var todoConnString = _spawnClient.GetConnectionString(_todoDataContainer, SpawnClient.EngineType.Postgres);
            var logger = Substitute.For<ILogger>();
            var todoConnectionService = new TodoConnectionService(todoConnString, true);
            var todoStore = new TodoStore(todoConnectionService, logger);
            var projectStore = new ProjectStore(todoConnectionService, logger);

            _todoController = new TodoController(todoStore, projectStore, logger);
            _todoController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = TestHelpers.GetUser(SmokeTestUserId) }
            };
        }

        [TearDown]
        public async Task Teardown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                GithubActionsHelpers.LogError($"Test '{TestContext.CurrentContext.Test.Name}' failed. Error: {TestContext.CurrentContext.Result.Message}");
            }
            // Don't wait for these tasks to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataContainer(_todoDataContainer));
            await Task.Delay(300);
        }

        [Test]
        public async Task WhenInsertingAndFindingTodoItem_ThenThisOperationIsCompleteInUnder500Ms()
        {
            var stopWatch = Stopwatch.StartNew();
            const string taskText = "my newly added todo item";
            var httpResult = await _todoController.RecordAsync(new Models.TodoItem()
            {
                CreatedAt = DateTime.Now,
                Done = true,
                Task = taskText,
            });
            Assert.That(httpResult, Is.InstanceOf(typeof(OkObjectResult)));

            var result = await _todoController.FindUserTodoItemAsync(taskText);
            stopWatch.Stop();

            Assert.That(stopWatch.ElapsedMilliseconds, Is.LessThan(500), "Finding a users todo item did not complete in less than 500ms");
        }
    }
}