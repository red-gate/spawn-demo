using System.Collections.Generic;
using System.Threading.Tasks;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public interface IAccountStore
    {
        Task<IEnumerable<Account>> GetAccountsAsync(string userId);
        Task<Account> GetAccountAsync(string userId);
        Task StoreAccountAsync(string userId, Account acc);
        Task RemoveAccountAsync(string userId, int id);
        Task ModifyAccountAsync(string userId, Account acc);
        Task<IEnumerable<Account>> GetAccountsByOrganizationAsync(string userId, int orgId);
    }
}