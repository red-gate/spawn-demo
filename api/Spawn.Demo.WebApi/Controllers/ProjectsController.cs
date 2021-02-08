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
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOrganizationStore _organizationStore;
        private readonly IProjectStore _store;


        public ProjectsController(IProjectStore store, IOrganizationStore organizationStore, ILogger logger)
        {
            _store = store;
            _organizationStore = organizationStore;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAsync()
        {
            return Ok(await _store.GetProjectsAsync(this.GetUserId()));
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Project>>> GetUserProjectsAsync()
        {
            return Ok(await _store.GetUserProjectsAsync(this.GetUserId()));
        }

        [HttpGet("organization/{orgId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetOrgProjectsAsync([FromRoute] int orgId)
        {
            return Ok(await _store.GetOrgProjectsAsync(this.GetUserId(), orgId));
        }

        [HttpPost]
        public async Task<IActionResult> RecordAsync([FromBody] Project project)
        {
            var userId = this.GetUserId();
            if (project.OrgId != null
                && _organizationStore.GetOrganizationAsync(userId, project.OrgId.Value) == null)
            {
                return BadRequest("organization not found");
            }

            var newProject = await _store.StoreProjectAsync(userId, project);
            return Ok(newProject);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] Project project)
        {
            var userId = this.GetUserId();
            if (await _store.GetProjectAsync(userId, project.Id) == null)
            {
                _logger.Information("project with id {id} for user {userId} not found", project.Id, userId);
                return NotFound();
            }

            if (project.OrgId != null && _organizationStore.GetOrganizationAsync(userId, project.OrgId.Value) == null)
            {
                return BadRequest();
            }

            await _store.RemoveProjectAsync(userId, project.Id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] Project project)
        {
            var userId = this.GetUserId();
            if (await _store.GetProjectAsync(userId, project.Id) == null)
            {
                return NotFound();
            }
            if (project.OrgId != null && _organizationStore.GetOrganizationAsync(userId, project.OrgId.Value) == null)
            {
                return BadRequest();
            }

            await _store.ModifyProjectAsync(userId, project);
            var updatedProject = await _store.GetProjectAsync(userId, project.Id);
            return Ok(updatedProject);
        }
    }
}