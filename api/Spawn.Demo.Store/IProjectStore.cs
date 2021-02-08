using System.Collections.Generic;
using System.Threading.Tasks;
using Spawn.Demo.Models;

namespace Spawn.Demo.Store
{
    public interface IProjectStore
    {
        Task<IEnumerable<Project>> GetProjectsAsync(string userId);
        Task<IEnumerable<Project>> GetUserProjectsAsync(string userId);
        Task<IEnumerable<Project>> GetOrgProjectsAsync(string userId, int orgId);

        Task<Project> GetProjectAsync(string userId, int id);
        Task<Project> StoreProjectAsync(string userId, Project project);
        Task RemoveProjectAsync(string userId, int id);
        Task ModifyProjectAsync(string userId, Project project);
    }
}