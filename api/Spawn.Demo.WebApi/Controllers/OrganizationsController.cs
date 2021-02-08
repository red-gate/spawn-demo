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
    public class OrganizationsController : ControllerBase
    {
        private readonly IAccountStore _accStore;
        private readonly ILogger _logger;
        private readonly IOrganizationStore _orgStore;

        public OrganizationsController(IOrganizationStore orgStore, IAccountStore accStore, ILogger logger)
        {
            _orgStore = orgStore;
            _accStore = accStore;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetUserOrganizationsAsync()
        {
            return Ok(await _orgStore.GetOrganizationsAsync(this.GetUserId()));
        }

        [HttpGet("{orgId}")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetUserOrganizationAsync([FromRoute] int orgId)
        {
            var org = await _orgStore.GetOrganizationAsync(this.GetUserId(), orgId);
            if (org == null)
            {
                return NotFound();
            }
            return Ok(org);
        }

        [HttpPost]
        public async Task<IActionResult> RecordAsync([FromBody] Organization org)
        {
            var userId = this.GetUserId();
            var orgId = await _orgStore.StoreOrganizationAsync(userId, org);
            var acc = await _accStore.GetAccountAsync(userId);
            var newOrg = await _orgStore.GetOrganizationAsync(userId, orgId);
            await _orgStore.JoinOrganizationAsync(acc, newOrg);
            return Ok(newOrg);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] Organization org)
        {
            var userId = this.GetUserId();
            if (await _orgStore.GetOrganizationAsync(userId, org.Id) == null)
            {
                _logger.Information("Org with id {id} for user {userId} not found", org.Id, userId);
                return NotFound();
            }

            await _orgStore.RemoveOrganizationAsync(userId, org.Id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] Organization org)
        {
            var userId = this.GetUserId();
            if (await _orgStore.GetOrganizationAsync(userId, org.Id) == null)
            {
                return NotFound();
            }
            await _orgStore.ModifyOrganizationAsync(userId, org);
            var updatedOrg = await _orgStore.GetOrganizationAsync(userId, org.Id);
            return Ok(updatedOrg);
        }

        [HttpPost("{orgId}/member")]
        public async Task<IActionResult> JoinAsync([FromRoute] int orgId)
        {
            var userId = this.GetUserId();

            var org = await _orgStore.GetOrganizationAsync(userId, orgId);
            var acc = await _accStore.GetAccountAsync(userId);
            if (org == null || acc == null)
            {
                return NotFound();
            }

            await _orgStore.JoinOrganizationAsync(acc, org);
            return Ok();
        }

        [HttpDelete("{orgId}/member")]
        public async Task<IActionResult> LeaveAsync([FromRoute] int orgId)
        {
            var userId = this.GetUserId();

            var org = await _orgStore.GetOrganizationAsync(userId, orgId);
            var acc = await _accStore.GetAccountAsync(userId);
            if (org == null || acc == null)
            {
                return NotFound();
            }

            await _orgStore.LeaveOrganizationAsync(acc, org);
            return Ok();
        }

        [HttpGet("{orgId}/members")]
        public async Task<ActionResult<IEnumerable<Account>>> MembersAsync([FromRoute] int orgId)
        {
            var userId = this.GetUserId();
            var org = await _orgStore.GetOrganizationAsync(userId, orgId);
            var acc = await _accStore.GetAccountAsync(userId);
            if (org == null || acc == null)
            {
                return NotFound();
            }

            var members = await _accStore.GetAccountsByOrganizationAsync(userId, org.Id);

            return Ok(members);
        }
    }
}