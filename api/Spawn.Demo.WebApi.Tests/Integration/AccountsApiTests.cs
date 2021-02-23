using NSubstitute;
using NUnit.Framework;
using Spawn.Demo.Store;
using Spawn.Demo.WebApi.Controllers;
using Spawn.Demo.WebApi.Tests.Spawnctl;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Interfaces;

namespace Spawn.Demo.WebApi.Tests
{
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    [Parallelizable(ParallelScope.All)]
    public class AccountsApiTests
    {
        private const string TestUserId = "test@example.com";
        private readonly SpawnClient _spawnClient;
        private AccountsController _accountsController;
        private string _accountsDataContainer = null;

        public AccountsApiTests()
        {
            _spawnClient = new SpawnClient(TestContext.Out);
        }

        [SetUp]
        public void Setup()
        {
            _accountsDataContainer = _spawnClient.CreateDataContainer(SetupFixture.AccountDataImageName);
            var connString = _spawnClient.GetConnectionString(_accountsDataContainer, SpawnClient.EngineType.MSSQL);
            var logger = Substitute.For<ILogger>();
            var accountConnectionService = new AccountConnectionService(connString, true);
            var accountStore = new AccountStore(accountConnectionService, logger);
            _accountsController = new AccountsController(accountStore, logger);

            _accountsController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = TestHelpers.GetUser(TestUserId) }
            };
        }

        [TearDown]
        public void Teardown()
        {
            if(TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
              _spawnClient.CreateImageFromCurrentContainerState(_accountsDataContainer, $"account-{TestContext.CurrentContext.Test.ID}", SetupFixture.AccountDataImageTag, "--team", "red-gate:sharks");
            }
            // Don't wait for this task to complete
            // We'll let spawn handle the background deletion
            Task.Run(() => _spawnClient.DeleteDataContainer(_accountsDataContainer));
        }

        [Test]
        public async Task WhenInvokingAddAccount_ThenOneAccountIsAdded()
        {
            var result = await _accountsController.GetAsync();
            var accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Empty);

            await _accountsController.RecordAsync(new Models.Account
            {
                CreatedAt = DateTime.Now,
                Email = TestUserId
            });

            result = await _accountsController.GetAsync();
            accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Not.Empty);
            Assert.That(accounts.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenInvokingDeleteAccount_ThenOneAccountIsDeleted()
        {
            await _accountsController.RecordAsync(new Models.Account
            {
                CreatedAt = DateTime.Now,
                Email = TestUserId
            });

            var result = await _accountsController.GetAsync();
            var accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Not.Empty);
            Assert.That(accounts.Count(), Is.EqualTo(1));

            var foundAccount = accounts.ElementAt(0);
            await _accountsController.DeleteAsync(foundAccount);

            result = await _accountsController.GetAsync();
            accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Empty);
        }

        [Test]
        public async Task WhenInvokingUpdateAccount_ThenOneAccountIsUpdated()
        {
            await _accountsController.RecordAsync(new Models.Account
            {
                CreatedAt = DateTime.Now,
                Email = TestUserId
            });

            var result = await _accountsController.GetAsync();
            var accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Not.Empty);
            Assert.That(accounts.Count(), Is.EqualTo(1));

            var foundAccount = accounts.ElementAt(0);

            const string updatedEmail = "newemail@example.com";
            foundAccount.Email = updatedEmail;

            await _accountsController.PutAsync(foundAccount);

            result = await _accountsController.GetAsync();
            accounts = TestHelpers.GetObjectResultContent<IEnumerable<Models.Account>>(result);
            Assert.That(accounts, Is.Not.Empty);
            Assert.That(accounts.Count(), Is.EqualTo(1));

            var updatedAccount = accounts.ElementAt(0);
            Assert.That(updatedAccount.Email, Is.EqualTo(updatedEmail));
        }
    }
}