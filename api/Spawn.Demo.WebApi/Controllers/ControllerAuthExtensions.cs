using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Spawn.Demo.WebApi.Controllers
{
    public static class ControllerAuthExtensions
    {
        public static string GetUserId(this ControllerBase controller)
        {
            var nameIdentifier = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            return nameIdentifier.Value;
        }
    }
}