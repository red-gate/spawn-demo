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
    public class ProjectsApiTests
    {
        private const string TestUserId = "test@example.com";
        private readonly SpawnClient _spawnClient;
        private ProjectsController _projectsController;
        private string _todoDataContainer = null;
        private string _accountDataContainer = null;

        public ProjectsApiTests()
        {
            _spawnClient = new SpawnClient(TestContext.Out);
        }

        [SetUp]
        public async Task Setup()
        {
            var createTodoContainerTask = Task.Run(() => {
              _todoDataContainer = _spawnClient.CreateDataContainer(SetupFixture.TodoDataImageName);
            });
            var createAccountContainerTask = Task.Run(() => {
              _accountDataContainer = _spawnClient.CreateDataContainer(SetupFixture.AccountDataImageName);
            });

            await Task.WhenAll(createTodoContainerTask, createAccountContainerTask);

            var todoConnString = _spawnClient.GetConnectionString(_todoDataContainer, SpawnClient.EngineType.Postgres);
            var accountConnString = _spawnClient.GetConnectionString(_accountDataContainer, SpawnClient.EngineType.MSSQL);
            var logger = Substitute.For<ILogger>();
            var todoConnectionService = new TodoConnectionService(todoConnString, true);
            var accountconnectionService = new AccountConnectionService(accountConnString, true);
            var projectStore = new ProjectStore(todoConnectionService, logger);
            var orgStore = new OrganizationStore(accountconnectionService, logger);
            
            _projectsController = new ProjectsController(projectStore, orgStore, logger);
            _projectsController.ControllerContext = new ControllerContext()
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
              _spawnClient.CreateImageFromCurrentContainerState(_accountDataContainer, $"account-{TestContext.CurrentContext.Test.ID}", "--tag", SetupFixture.AccountDataImageTag, "--team", "red-gate:sharks");
            }
            // Don't wait for these tasks to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataContainer(_todoDataContainer));
            Task.Run(() => _spawnClient.DeleteDataContainer(_accountDataContainer));
        }

        [Test]
        public async Task WhenInvokingAddProject_ThenOneProjectIsAdded()
        {
            var result = await _projectsController.GetUserProjectsAsync();
            var projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Empty);

            await _projectsController.RecordAsync(new Models.Project
            {
              CreatedAt = DateTime.Now,
              Name = "TestProject1",
            });

            result = await _projectsController.GetUserProjectsAsync();
            projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Not.Empty);
            Assert.That(projects.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenInvokingDeleteProject_ThenOneProjectIsDeleted()
        {
            await _projectsController.RecordAsync(new Models.Project
            {
              CreatedAt = DateTime.Now,
              Name = "TestProject1",
            });

            var result = await _projectsController.GetUserProjectsAsync();
            var projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Not.Empty);

            var foundProject = projects.ElementAt(0);
            await _projectsController.DeleteAsync(foundProject);

            result = await _projectsController.GetUserProjectsAsync();
            projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Empty);
        }

        [Test]
        public async Task WhenInvokingUpdateProject_ThenOneProjectIsUpdated()
        {
            await _projectsController.RecordAsync(new Models.Project
            {
              CreatedAt = DateTime.Now,
              Name = "TestProject1",
            });

            var result = await _projectsController.GetUserProjectsAsync();
            var projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Not.Empty);

            var foundProject = projects.ElementAt(0);

            const string updatedName = "UpdatedTestProject1";
            foundProject.Name = updatedName;

            await _projectsController.PutAsync(foundProject);

            result = await _projectsController.GetAsync();
            projects = TestHelpers.GetObjectResultContent<IEnumerable<Models.Project>>(result);
            Assert.That(projects, Is.Not.Empty);
            Assert.That(projects.Count(), Is.EqualTo(1));

            var updatedProject = projects.ElementAt(0);
            Assert.That(updatedProject.Name, Is.EqualTo(updatedName));
        }
    }
}