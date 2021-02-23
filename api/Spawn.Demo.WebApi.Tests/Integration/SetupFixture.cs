
using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Spawn.Demo.WebApi.Tests.Spawnctl;

namespace Spawn.Demo.WebApi.Tests
{
    [SetUpFixture]
    public class SetupFixture
    {
        private readonly SpawnClient _spawnClient;
        public static string AccountDataImageName = null;
        public static string TodoDataImageName = null;

        public SetupFixture()
        {
            _spawnClient = new SpawnClient(TestContext.Out);
        }

        [OneTimeSetUp]
        public async Task CreateDataImages()
        {
            var accountImageName = Environment.GetEnvironmentVariable("ACCOUNT_IMAGE_NAME");
            var todoImageName = Environment.GetEnvironmentVariable("TODO_IMAGE_NAME");

            AccountDataImageName = string.IsNullOrEmpty(accountImageName) ? "demo-account-test" : accountImageName;
            TodoDataImageName = string.IsNullOrEmpty(todoImageName) ? "demo-todo-test" : todoImageName;

            var accountImageYamlFilepath = Environment.GetEnvironmentVariable("ACCOUNT_IMAGE_YAML_FILEPATH");
            var todoImageYamlFilepath = Environment.GetEnvironmentVariable("TODO_IMAGE_YAML_FILEPATH");

            accountImageYamlFilepath = string.IsNullOrEmpty(accountImageYamlFilepath) ? "../../../../../database/account/spawn/test.yaml" : accountImageYamlFilepath;
            todoImageYamlFilepath = string.IsNullOrEmpty(todoImageYamlFilepath) ? "../../../../../database/todo/spawn/test.yaml" : todoImageYamlFilepath;

            var createAccount = Task.Run(() => _spawnClient.CreateDataImage(accountImageYamlFilepath, "-n", AccountDataImageName));
            var createTodo = Task.Run(() => _spawnClient.CreateDataImage(todoImageYamlFilepath, "-n", TodoDataImageName));
            await Task.WhenAll(createAccount, createTodo);
        }

        [OneTimeTearDown]
        public void DeleteDataImages()
        {
            // Don't wait for these tasks to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataImage(AccountDataImageName));
            Task.Run(() => _spawnClient.DeleteDataImage(TodoDataImageName));
        }
    }
}