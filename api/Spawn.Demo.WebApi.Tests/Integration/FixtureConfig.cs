
using NUnit.Framework;
using Spawn.Demo.WebApi.Tests.Spawnctl;

[assembly: LevelOfParallelism(10)]
namespace Spawn.Demo.WebApi.Tests
{
    public class FixtureConfig
    {
        public static string AccountDataImageIdentifier = "demo-account:latest";
        public static string TodoDataImageIdentifier = "demo-todo:latest";
        public static string TestTag = null;

        public FixtureConfig()
        {
            var testTag = Environment.GetEnvironmentVariable("TEST_TAG");
            if (string.IsNullOrEmpty(testTag))
            {
                TestTag = "localtest";
            }
            else
            {
                TestTag = testTag;
            }
        }
    }
}