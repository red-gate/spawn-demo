using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Serilog;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public class AccountStore : StoreRetrier, IAccountStore
    {
        private readonly string _connString;
        private readonly ILogger _logger;

        public AccountStore(AccountConnectionService connectionService, ILogger logger)
        {
            _logger = logger;
            _connString = connectionService.ConnString;
        }

        public async Task RemoveAccountAsync(string userId, int id)
        {
            await RunWithRetryAsync(() => DeleteAccount(userId, id));
        }

        public async Task<Account> GetAccountAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectAccount(userId));
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectAccounts(userId));
        }

        public async Task StoreAccountAsync(string userId, Account acc)
        {
            await RunWithRetryAsync(() => InsertAccount(userId, acc));
        }

        public async Task ModifyAccountAsync(string userId, Account acc)
        {
            await RunWithRetryAsync(() => UpdateAccount(userId, acc));
        }

        public async Task<IEnumerable<Account>> GetAccountsByOrganizationAsync(string userId, int orgId)
        {
            return await RunWithRetryAsync(() => SelectAccountsByOrganization(userId, orgId));
        }

        private void DeleteAccount(string userId, int id)
        {
            using (IDbConnection conn = new SqlConnection(_connString))
            {
                var rows = conn.Execute(
                    @"DELETE FROM accounts
WHERE UserID = @userId
AND ID = @id;", new {userId, id});
                _logger.Information("Accounts removed {count} records from accounts", rows);
            }
        }

        private Account SelectAccount(string userId)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var account = conn.QuerySingleOrDefault<Account>(
                    @"SELECT ID, UserID, Email, CreatedAt
FROM Accounts
WHERE UserID = @userId;", new {userId});
                return account;
            }
        }

        private IEnumerable<Account> SelectAccounts(string userId)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var accounts = conn.Query<Account>(
                    @"SELECT ID, UserID, Email, CreatedAt
FROM Accounts
WHERE UserID = @userId;", new {userId});
                _logger.Information("Accounts found {count} records", accounts.Count());
                return accounts;
            }
        }

        private void InsertAccount(string userId, Account acc)
        {
            using (var conn = new SqlConnection(_connString))
            {
                if (AccountExists(userId, conn))
                {
                    _logger.Information("Account already exists, not creating...");
                    return;
                }
            
                var id = conn.ExecuteScalar<int>(
                    @"INSERT INTO Accounts(UserID, Email)
VALUES (@userId, @email);
SELECT SCOPE_IDENTITY();",
                    new
                    {
                        userId,
                        email = acc.Email
                    });
                _logger.Information("accounts inserted id {id}", id);
            }
        }

        private void UpdateAccount(string userId, Account acc)
        {
            using (IDbConnection connection = new SqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(@"
UPDATE Accounts
SET
    Email = @email
WHERE UserID = @userId
AND ID = @id;", new
                {
                    userId,
                    email = acc.Email,
                    id = acc.Id
                });
                _logger.Information("Accounts updated id {id}", id);
            }
        }

        private IEnumerable<Account> SelectAccountsByOrganization(string userId, int orgId)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var accounts = conn.Query<Account>(
                    @"
SELECT a.ID, a.UserID, a.Email, a.CreatedAt
FROM accounts AS a
INNER JOIN organizations_members AS m ON m.AccountID =  a.ID
WHERE a.UserID = @userId
AND m.OrgID = @orgId;",
                    new {userId, orgId});
                _logger.Information("found {count} accounts for user {userId} and organization {orgId}",
                    accounts.Count(), userId, orgId);
                return accounts;
            }
        }
        
        private bool AccountExists(string userId, IDbConnection conn)
        {
            var exists = conn.ExecuteScalar<bool>(@"
SELECT CASE WHEN EXISTS (
    SELECT *
    FROM accounts
    WHERE UserID = @userId)
THEN CAST(1 AS BIT)
ELSE CAST(0 AS BIT) END", new {userId});
            return exists;
        }

        protected override bool CanConnect()
        {
            try
            {
                using (IDbConnection connection = new SqlConnection(_connString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to connect to db");
                return false;
            }
        }
    }
}