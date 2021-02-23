using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace Spawn.Demo.WebApi.Tests.Spawnctl
{
    public class SpawnClient
    {
        private readonly TextWriter _logger;

        public SpawnClient(TextWriter logger)
        {
            _logger = logger;
        }

        public string CreateDataImage(string imageDefinitionFilepath, string imageName, string tag, params string[] extraArgs)
        {
            _logger.WriteLine("🛸 Creating spawn data image...");

            var args = new List<string> { "create", "data-image", "-f", imageDefinitionFilepath, "--name", imageName, "--tag", tag };
            args.AddRange(extraArgs);
            var dataImage = RunSpawnctl(args.ToArray());
            _logger.WriteLine($"🛸 Successfully created spawn data image '{imageName}:{tag}'");
            return dataImage;
        }

        public string CreateDataContainer(string imageIdentifier)
        {
            _logger.WriteLine($"🛸 Creating spawn data container from image '{imageIdentifier}...");
            var dataContainer = RunSpawnctl("create", "data-container", "--image", imageIdentifier);
            _logger.WriteLine($"🛸 Successfully created spawn data container '{dataContainer}'");
            return dataContainer;
        }

        public string DeleteDataImage(string imageIdentifier)
        {
            _logger.WriteLine($"🛸 Deleting spawn data image '{imageIdentifier}'...");
            var result = RunSpawnctl("delete", "data-image", imageIdentifier);
            _logger.WriteLine($"🛸 Successfully deleted spawn data image {imageIdentifier}");
            return result;
        }

        public string DeleteDataContainer(string containerIdentifier)
        {
            _logger.WriteLine($"🛸 Deleting spawn data container '{containerIdentifier}'...");
            var result = RunSpawnctl("delete", "data-container", containerIdentifier);
            _logger.WriteLine($"🛸 Successfully deleted spawn data container '{containerIdentifier}'");
            return result;
        }

        public string GetConnectionString(string containerIdentifier, EngineType engineType)
        {
            var dataContainerDetailsJson = RunSpawnctl("get", "data-container", containerIdentifier, "-o", "json");
            var connDetails = JsonConvert.DeserializeObject<ConnectionDetails>(dataContainerDetailsJson);

            switch (engineType)
            {
                case EngineType.MSSQL:
                    return GetMssqlConnString(connDetails);
                case EngineType.Postgres:
                    return GetPgConnString(connDetails);
                default:
                    throw new Exception($"Unknown engine {engineType}");
            }
        }

        public string CreateImageFromCurrentContainerState(string containerIdentifier, string newImageName, string tag, params string[] extraArgs)
        {
            _logger.WriteLine($"🛸 Creating new data image '{newImageName}' from current state of data container '{containerIdentifier}'...");
            var newRevision = RunSpawnctl("save", "data-container", containerIdentifier, "-q");
            var args = new List<string> { "graduate", "data-container", containerIdentifier, "--revision", newRevision, "--name", newImageName, "--tag", tag };
            args.AddRange(extraArgs);
            var newImage = RunSpawnctl(args.ToArray());
            _logger.WriteLine($"🛸 Successfully graduated current data container state to new data image '{newImageName}:{tag}'");
            return newImage;
        }

        private string GetPgConnString(ConnectionDetails connectionDetails)
        {
            var npgsqlConnStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = connectionDetails.Host,
                Port = int.Parse(connectionDetails.Port),
                Username = connectionDetails.User,
                Password = connectionDetails.Password,
                TrustServerCertificate = true,
                Database = "spawndemotodo"
            };
            return npgsqlConnStringBuilder.ToString();
        }

        private string GetMssqlConnString(ConnectionDetails connectionDetails)
        {
            var mssqlConnStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = $"{connectionDetails.Host},{connectionDetails.Port}",
                UserID = connectionDetails.User,
                Password = connectionDetails.Password,
                TrustServerCertificate = true,
                InitialCatalog = "spawndemoaccount"
            };
            return mssqlConnStringBuilder.ToString();
        }

        private string RunSpawnctl(params string[] arguments)
        {
            var args = arguments.Append("-q");
            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "spawnctl",
                    Arguments = string.Join(' ', args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().TrimEnd();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd().TrimEnd();
                throw new Exception($"Exit code {process.ExitCode}. Logs: {error}");
            }

            return output;
        }

        private class ConnectionDetails
        {
            public string Host { get; set; }
            public string User { get; set; }
            public string Port { get; set; }
            public string Password { get; set; }
        }
        public enum EngineType
        {
            MSSQL,
            Postgres
        }
    }
}