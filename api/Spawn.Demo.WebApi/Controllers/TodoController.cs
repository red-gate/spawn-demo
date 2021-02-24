using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Spawn.Demo.Models;
using Spawn.Demo.Store;

namespace Spawn.Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IProjectStore _projectStore;
        private readonly ITodoStore _todoStore;

        public TodoController(ITodoStore todoStore, IProjectStore projectStore, ILogger logger)
        {
            _todoStore = todoStore;
            _projectStore = projectStore;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetAsync()
        {
            return Ok(await _todoStore.GetTodoItemsAsync(this.GetUserId()));
        }

        [HttpGet("/user/{taskText}")]
        public async Task<ActionResult<TodoItem>> FindUserTodoItemAsync([FromRoute]string taskText)
        {
            return Ok(await _todoStore.FindUserTodoItemsAsync(this.GetUserId(), taskText));
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetUserTodoItemsAsync()
        {
            return Ok(await _todoStore.GetUserTodoItemsAsync(this.GetUserId()));
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetProjectTodoItemsAsync([FromRoute] int projectId)
        {
            var userId = this.GetUserId();
            if (await _projectStore.GetProjectAsync(userId, projectId) == null)
            {
                return NotFound();
            }
            return Ok(await _todoStore.GetProjectTodoItemsAsync(userId, projectId));
        }

        [HttpPost]
        public async Task<IActionResult> RecordAsync([FromBody] TodoItem item)
        {
            var userId = this.GetUserId();

            if (item.ProjectId != null)
            {
                var project = await _projectStore.GetProjectAsync(userId, item.ProjectId.Value);
                if (project == null)
                {
                    return NotFound();
                }
            }

            var newItem = await _todoStore.StoreTodoItemAsync(userId, item);
            return Ok(newItem);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] TodoItem item)
        {
            var userId = this.GetUserId();
            if (await _todoStore.GetTodoItemAsync(userId, item.Id) == null)
            {
                _logger.Information("Item with id {id} for user {userId} not found", item.Id, userId);
                return NotFound();
            }

            await _todoStore.RemoveTodoItemAsync(userId, item.Id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] TodoItem item)
        {
            var userId = this.GetUserId();
            if (await _todoStore.GetTodoItemAsync(userId, item.Id) == null)
            {
                return NotFound();
            }
            await _todoStore.ModifyTodoItemAsync(userId, item);
            var newItem = await _todoStore.GetTodoItemAsync(userId, item.Id);
            return Ok(newItem);
        }
    }
}