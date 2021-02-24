using System;
using NUnit.Framework;

namespace Spawn.Demo.WebApi.Tests
{
    public static class GithubActionsHelpers
    {
        public static void LogError(string message)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKFLOW")))
            {
                TestContext.WriteLine($"##[error]{message}");
            }
        }
    }
}