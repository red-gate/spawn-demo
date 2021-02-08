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
    public class TodoStore : StoreRetrier, ITodoStore
    {
        private readonly string _connString;

        private readonly ILogger _logger;

        public TodoStore(TodoConnectionService todoConnectionService, ILogger logger)
        {
            _connString = todoConnectionService.ConnString;
            _logger = logger;
        }

        public async Task<TodoItem> StoreTodoItemAsync(string userId, TodoItem item)
        {
            return await RunWithRetryAsync(() => InsertTodoItem(userId, item));
        }

        public async Task ModifyTodoItemAsync(string userId, TodoItem item)
        {
            await RunWithRetryAsync(() => UpdateTodoItem(userId, item));
        }

        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectTodoItems(userId));
        }

        public async Task<TodoItem> GetTodoItemAsync(string userId, int id)
        {
            return await RunWithRetryAsync(() => SelectTodoItem(userId, id));
        }

        public async Task<IEnumerable<TodoItem>> GetUserTodoItemsAsync(string userId)
        {
            return await RunWithRetryAsync(() => SelectUserTodoItems(userId));
        }

        public async Task<IEnumerable<TodoItem>> GetProjectTodoItemsAsync(string userId, int projectId)
        {
            return await RunWithRetryAsync(() => SelectProjectTodoItems(userId, projectId));
        }

        public async Task RemoveTodoItemAsync(string userId, int id)
        {
            await RunWithRetryAsync(() => DeleteTodoItem(userId, id));
        }

        private TodoItem InsertTodoItem(string userId, TodoItem item)
        {
            using (IDbConnection connection = new NpgsqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(@"
INSERT INTO todo_list(userId, task, done, projectId)
VALUES (@userId, @task, @done, @projectId) RETURNING Id;", new
                {
                    userId,
                    task = item.Task,
                    done = item.Done,
                    projectId = item.ProjectId
                });
                var newItem = SelectTodoItem(userId, id);
                _logger.Information("inserted id {id}", id);
                return newItem;
            }
        }

        private void UpdateTodoItem(string userId, TodoItem item)
        {
            using (IDbConnection connection = new NpgsqlConnection(_connString))
            {
                var id = connection.ExecuteScalar<int>(@"
UPDATE todo_list
SET
    task = @task,
    done = @done
WHERE userId = @userId
AND id = @id;", new
                {
                    userId,
                    task = item.Task,
                    done = item.Done,
                    id = item.Id
                });
                _logger.Information("Updated id {id}", id);
            }
        }

        private IEnumerable<TodoItem> SelectTodoItems(string userId)
        {
            using (IDbConnection conn = new NpgsqlConnection(_connString))
            {
                var todoItems = conn.Query<TodoItem>(
                    @"SELECT id, userId, task, done, createdAt, projectId
FROM todo_list
WHERE userId = @userId;", new {userId});
                _logger.Information("Found {count} records", todoItems.Count());
                return todoItems;
            }
        }

        private TodoItem SelectTodoItem(string userId, int id)
        {
            using (var conn = new NpgsqlConnection(_connString))
            {
                var todoItem = conn.QuerySingleOrDefault<TodoItem>(
                    @"SELECT id, userId, task, done, createdAt, projectId
FROM todo_list
WHERE userId = @userId
AND id = @id;", new {userId, id});
                return todoItem;
            }
        }


        private IEnumerable<TodoItem> SelectUserTodoItems(string userId)
        {
            using (var conn = new NpgsqlConnection(_connString))
            {
                var todoItem = conn.Query<TodoItem>(
                    @"SELECT id, userId, task, done, createdAt, projectId
FROM todo_list
WHERE userId = @userId
AND projectId IS NULL;", new {userId});
                return todoItem;
            }
        }

        private IEnumerable<TodoItem> SelectProjectTodoItems(string userId, int projectId)
        {
            using (var conn = new NpgsqlConnection(_connString))
            {
                var todoItem = conn.Query<TodoItem>(
                    @"SELECT id, userId, task, done, createdAt, projectId
FROM todo_list
WHERE userId = @userId
AND projectId = @projectId;", new {userId, projectId});
                return todoItem;
            }
        }

        private void DeleteTodoItem(string userId, int id)
        {
            using (IDbConnection conn = new NpgsqlConnection(_connString))
            {
                var rows = conn.Execute(
                    @"DELETE FROM todo_list
WHERE userId = @userId;", new {userId, id});
                _logger.Information("Removed {count} records from todo_list", rows);
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
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to connect to db");
                return false;
            }
        }
    }
}