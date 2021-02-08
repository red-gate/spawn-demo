using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Serilog;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public class ProjectStore : StoreRetrier, IProjectStore
    {
        private readonly string _connString;
        private readonly ILogger _logger;

        public ProjectStore(TodoConnectionService todoConnectionService, ILogger logger)
        {
            _logger = logger;
            _connString = todoConnectionService.ConnString;
        }

        public async Task RemoveProjectAsync(string userId, int id)
        {
            await RunWithRetryAsync(() => DeleteProject(userId, id));
        }

        public async Task<IEnumerable<Project>> GetOrgProjectsAsync(string userId, int orgId)
        {
            return await RunWithRetryAsync(() => SelectOrgProjects(userId, orgId));
        }

        public async Task<Project> GetProjectAsync(string userId, int id)
        {
            return await RunWithRetryAsync(() => SelectProject(userId, id));
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectProjects(userId));
        }

        public async Task<IEnumerable<Project>> GetUserProjectsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectUserProjects(userId));
        }


        public async Task<Project> StoreProjectAsync(string userId, Project project)
        {
            return await RunWithRetryAsync(() => InsertProject(userId, project));
        }


        public async Task ModifyProjectAsync(string userId, Project project)
        {
            await RunWithRetryAsync(() => UpdateProject(userId, project));
        }

        private void DeleteProject(string userId, int id)
        {
            using (IDbConnection conn = new NpgsqlConnection(_connString))
            {
                var rows = conn.Execute(
                    @"
DELETE FROM todo_list
WHERE userId = @userId
AND projectId = @id;

DELETE FROM projects
WHERE userId = @userId
AND id = @id;", new {userId, id});
                _logger.Information("Projects - removed {count} records from projects", rows);
            }
        }

        private IEnumerable<Project> SelectOrgProjects(string userId, int orgId)
        {
            using (var conn = new NpgsqlConnection(_connString))
            {
                var projects = conn.Query<Project>(
                    @"SELECT id, userId, name, createdAt, orgId
FROM projects
WHERE userId = @userId
AND orgId = @orgId;", new {userId, orgId});
                return projects;
            }
        }

        private Project SelectProject(string userId, int id)
        {
            using (var conn = new NpgsqlConnection(_connString))
            {
                var project = conn.QuerySingleOrDefault<Project>(
                    @"SELECT id, userId, name, createdAt, orgId
FROM projects
WHERE userId = @userId
AND id = @id;", new {userId, id});
                return project;
            }
        }

        private IEnumerable<Project> SelectProjects(string userId)
        {
            using (IDbConnection conn = new NpgsqlConnection(_connString))
            {
                var projects = conn.Query<Project>(
                    @"SELECT id, userId, name, createdAt, orgId
FROM projects
WHERE userId = @userId;", new {userId});
                _logger.Information("Projects - found {count} records", projects.Count());
                return projects;
            }
        }

        private IEnumerable<Project> SelectUserProjects(string userId)
        {
            using (IDbConnection conn = new NpgsqlConnection(_connString))
            {
                var projects = conn.Query<Project>(
                    @"SELECT id, userId, name, createdAt, orgId
FROM projects
WHERE userId = @userId
AND orgId is NULL;", new {userId});
                _logger.Information("Projects - found {count} records", projects.Count());
                return projects;
            }
        }

        private Project InsertProject(string userId, Project project)
        {
            using (IDbConnection connection = new NpgsqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(@"
INSERT INTO projects(userId, name, orgId)
VALUES (@userId, @name, @orgId) RETURNING Id;", new
                {
                    userId,
                    name = project.Name,
                    project.OrgId
                });
                _logger.Information("Projects - inserted id {id}", id);
                return SelectProject(userId, id);
            }
        }

        private void UpdateProject(string userId, Project project)
        {
            using (IDbConnection connection = new NpgsqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(@"
UPDATE projects
SET
    name = @name
WHERE userId = @userId
AND id = @id;", new
                {
                    userId,
                    name = project.Name,
                    id = project.Id
                });
                _logger.Information("Projects - Updated id {id}", id);
            }
        }

        protected override bool CanConnect()
        {
            try
            {
                using (IDbConnection connection = new NpgsqlConnection(_connString))
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