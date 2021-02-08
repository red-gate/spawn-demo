using System.Collections.Generic;
using System.Threading.Tasks;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public interface IOrganizationStore
    {
        Task<IEnumerable<Organization>> GetOrganizationsAsync(string userId);
        Task<Organization> GetOrganizationAsync(string userId, int id);
        Task<int> StoreOrganizationAsync(string userId, Organization org);
        Task RemoveOrganizationAsync(string userId, int id);
        Task ModifyOrganizationAsync(string userId, Organization org);
        Task JoinOrganizationAsync(Account account, Organization org);
        Task LeaveOrganizationAsync(Account account, Organization org);
    }
}