using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Spawn.Demo.WebApi.Tests
{
    internal static class TestHelpers
    {
        internal static T GetObjectResultContent<T>(ActionResult<T> result)
        {
            return (T)((ObjectResult)result.Result).Value;
        }

        internal static ClaimsPrincipal GetUser(string name)
        {
            var user = new ClaimsPrincipal();
            user.AddIdentities(new[]
            {
              new ClaimsIdentity(new[]
              {
                new Claim(ClaimTypes.NameIdentifier, name)
              })
            });

            return user;
        }
    }
}