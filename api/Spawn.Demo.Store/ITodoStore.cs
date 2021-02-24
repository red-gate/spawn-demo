using System.Collections.Generic;
using System.Threading.Tasks;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public interface ITodoStore
    {
        Task<IEnumerable<TodoItem>> GetTodoItemsAsync(string userId);
        Task<IEnumerable<TodoItem>> GetUserTodoItemsAsync(string userId);
        Task<IEnumerable<TodoItem>> GetProjectTodoItemsAsync(string userId, int projectId);
        Task<TodoItem> GetTodoItemAsync(string userId, int id);
        Task<TodoItem> StoreTodoItemAsync(string userId, TodoItem item);
        Task RemoveTodoItemAsync(string userId, int id);
        Task ModifyTodoItemAsync(string userId, TodoItem item);
        Task<TodoItem> FindUserTodoItemsAsync(string userId, string taskText);
    }
}