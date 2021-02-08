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
    public class AccountsController : ControllerBase
    {
        public AccountsController(IAccountStore store, ILogger logger)
        {
            Store = store;
            Logger = logger;
        }

        private IAccountStore Store { get; }
        private ILogger Logger { get; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAsync()
        {
            return Ok(await Store.GetAccountsAsync(this.GetUserId()));
        }

        [HttpPost]
        public async Task<IActionResult> RecordAsync([FromBody] Account acc)
        {
            var userId = this.GetUserId();
            await Store.StoreAccountAsync(userId, acc);
            var createdAccount = await Store.GetAccountAsync(userId);
            return Ok(createdAccount);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] Account Acc)
        {
            var userId = this.GetUserId();
            if (await Store.GetAccountAsync(userId) == null)
            {
                Logger.Information("Account with id {id} for user {userId} not found", Acc.Id, userId);
                return NotFound();
            }

            await Store.RemoveAccountAsync(userId, Acc.Id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] Account Acc)
        {
            var userId = this.GetUserId();
            if (await Store.GetAccountAsync(userId) == null) return NotFound();
            await Store.ModifyAccountAsync(userId, Acc);
            var updatedAccount = await Store.GetAccountAsync(userId);
            return Ok(updatedAccount);
        }
    }
}