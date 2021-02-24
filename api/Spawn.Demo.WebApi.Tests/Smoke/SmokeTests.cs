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
            _spawnClient = new SpawnClient(TestContext.Out);
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
            // Don't wait for these tasks to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataContainer(_todoDataContainer));
            await Task.Delay(300);
        }

        [Test]
        public async Task WhenInsertingAndFindingTodoItem_ThenThisOperationIsCompleteInUnderFiveSecond()
        {
            const string existingTaskText = "this is my task text to find";
            var stopWatch = Stopwatch.StartNew();
            var result = await _todoController.FindUserTodoItemAsync(existingTaskText);
            stopWatch.Stop();
            
            Assert.That(stopWatch.ElapsedMilliseconds, Is.LessThan(3000), "Finding a users todo item did not complete in less than 5 second");
        }
   }
}