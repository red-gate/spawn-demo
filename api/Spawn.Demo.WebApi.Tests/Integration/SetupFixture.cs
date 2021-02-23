
using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Spawn.Demo.WebApi.Tests.Spawnctl;

[assembly: LevelOfParallelism(10)]
namespace Spawn.Demo.WebApi.Tests
{
    [SetUpFixture]
    public class SetupFixture
    {
        private const string DefaultAccountYamlFilepath = "../../../../../database/account/spawn/test.yaml";
        private const string DefaultTodoYamlFilepath = "../../../../../database/todo/spawn/test.yaml";
        private readonly SpawnClient _spawnClient;
        public static string AccountDataImageName = null;
        public static string TodoDataImageName = null;
        public static string AccountDataImageTag = null;
        public static string TodoDataImageTag = null;

        public SetupFixture()
        {
            _spawnClient = new SpawnClient(TestContext.Progress);
        }

        [OneTimeSetUp]
        public async Task CreateDataImages()
        {
            var accountImageName = Environment.GetEnvironmentVariable("ACCOUNT_IMAGE_NAME");
            var todoImageName = Environment.GetEnvironmentVariable("TODO_IMAGE_NAME");

            accountImageName = string.IsNullOrEmpty(accountImageName) ? "demo-account-test" : accountImageName;
            todoImageName = string.IsNullOrEmpty(todoImageName) ? "demo-todo-test" : todoImageName;

            var accountTagName = Environment.GetEnvironmentVariable("ACCOUNT_TAG_NAME");
            var todoTagName = Environment.GetEnvironmentVariable("TODO_TAG_NAME");

            accountTagName = string.IsNullOrEmpty(accountTagName) ? "local" : accountTagName;
            todoTagName = string.IsNullOrEmpty(todoTagName) ? "local" : todoTagName;

            var accountImageYamlFilepath = Environment.GetEnvironmentVariable("ACCOUNT_IMAGE_YAML_FILEPATH");
            var todoImageYamlFilepath = Environment.GetEnvironmentVariable("TODO_IMAGE_YAML_FILEPATH");

            accountImageYamlFilepath = string.IsNullOrEmpty(accountImageYamlFilepath) ? DefaultAccountYamlFilepath : accountImageYamlFilepath;
            todoImageYamlFilepath = string.IsNullOrEmpty(todoImageYamlFilepath) ? DefaultTodoYamlFilepath : todoImageYamlFilepath;

            var createAccount = Task.Run(() => _spawnClient.CreateDataImage(accountImageYamlFilepath, accountImageName, accountTagName));
            var createTodo = Task.Run(() => _spawnClient.CreateDataImage(todoImageYamlFilepath, todoImageName, todoTagName));
            await Task.WhenAll(createAccount, createTodo);

            AccountDataImageName = $"{accountImageName}:{accountTagName}";
            TodoDataImageName = $"{todoImageName}:{todoTagName}";
            AccountDataImageTag = accountTagName;
            TodoDataImageTag = todoTagName;
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