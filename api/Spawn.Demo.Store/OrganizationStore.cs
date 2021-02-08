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
    public class OrganizationStore : StoreRetrier, IOrganizationStore
    {
        private readonly string _connString;
        private readonly ILogger _logger;

        public OrganizationStore(AccountConnectionService connectionService, ILogger logger)
        {
            _connString = connectionService.ConnString;
            _logger = logger;
        }

        public async Task RemoveOrganizationAsync(string userId, int id)
        {
            await RunWithRetryAsync(() => DeleteOrganization(userId, id));
        }

        public async Task<Organization> GetOrganizationAsync(string userId, int id)
        {
            return await RunWithRetryAsync(() => SelectOrganization(userId, id));
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectOrganizations(userId));
        }

        public async Task<int> StoreOrganizationAsync(string userId, Organization org)
        {
            return await RunWithRetryAsync(() => InsertOrganization(userId, org));
        }

        public async Task ModifyOrganizationAsync(string userId, Organization org)
        {
            await RunWithRetryAsync(() => UpdateOrganization(userId, org));
        }

        public async Task JoinOrganizationAsync(Account account, Organization org)
        {
            await RunWithRetryAsync(() => InsertIntoOrganization(account, org));
        }

        public async Task LeaveOrganizationAsync(Account account, Organization org)
        {
            await RunWithRetryAsync(() => DeleteFromOrganization(account, org));
        }

        private void DeleteOrganization(string userId, int id)
        {
            using (IDbConnection conn = new SqlConnection(_connString))
            {
                var rows = conn.Execute(
                    @"
DELETE FROM organizations_members
WHERE OrgID = @id;

DELETE FROM organizations
WHERE ID = @id;", new {id});
                _logger.Information("Organizations removed {count} records", rows);
            }
        }

        private Organization SelectOrganization(string userId, int id)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var org = conn.QuerySingleOrDefault<Organization>(
                    @"SELECT ID, Name, CreatedAt
FROM Organizations
WHERE ID = @id;", new {userId, id});
                return org;
            }
        }

        private IEnumerable<Organization> SelectOrganizations(string userId)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var orgs = conn.Query<Organization>(
                    @"SELECT o.ID, o.Name, o.CreatedAt
FROM organizations AS o, organizations_members as m, accounts as a
WHERE o.ID = m.OrgID
AND a.ID = m.AccountID
AND a.UserID = @userId;", new {userId});
                _logger.Information("Organizations found {count} records", orgs.Count());
                return orgs;
            }
        }

        private int InsertOrganization(string userId, Organization org)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var id = conn.ExecuteScalar<int>(
                    @"INSERT INTO Organizations(Name)
VALUES (@name);
SELECT SCOPE_IDENTITY();",
                    new
                    {
                        name = org.Name
                    });
                _logger.Information("Organizations inserted id {id}", id);
                return id;
            }
        }

        private void UpdateOrganization(string userId, Organization org)
        {
            using (IDbConnection connection = new SqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(
                    @"UPDATE Organizations
SET
    Name = @name
WHERE ID = @id;",
                    new
                    {
                        name = org.Name,
                        id = org.Id
                    });
                _logger.Information("Organizations updated id {id}", id);
            }
        }

        private void InsertIntoOrganization(Account account, Organization org)
        {
            using (var conn = new SqlConnection(_connString))
            {
                _logger.Information("Acc {id} org {id}", account.Id, org.Id);
                var id = conn.ExecuteScalar<int>(
                    @"INSERT INTO organizations_members(OrgID, AccountID)
            VALUES (@orgId, @accountId);
            SELECT SCOPE_IDENTITY();",
                    new
                    {
                        accountId = account.Id,
                        orgId = org.Id
                    });
                _logger.Information("Organizations member inserted id {id}", id);
            }
        }

        private void DeleteFromOrganization(Account account, Organization org)
        {
            using (var conn = new SqlConnection(_connString))
            {
                var id = conn.ExecuteScalar<int>(
                    @"
DELETE FROM organizations_members
WHERE OrgID = @orgId
AND AccountID = @accountId;",
                    new
                    {
                        accountId = account.Id,
                        orgId = org.Id
                    });
                _logger.Information("Organizations member inserted id {id}", id);
            }
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}