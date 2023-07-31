using LibGit2Sharp;
using RobloxTrackingCentral.Trackers.Api.Docs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs
{
    internal class WorkerFactory
    {
        private const string BaseUrl = "roblox.com";
        private const string SwaggerConfigurationPrefix = "            var configObject = JSON.parse('";

        private Repository _Repository;
        private Queue<string> _APIs;

        public List<string> Changes { get; }

        public WorkerFactory(Repository repository, Queue<string> apis)
        {
            _Repository = repository;
            _APIs = apis;

            Changes = new List<string>();
        }

        private async Task<List<VersionInformation>> GetAllApiVersions(string api)
        {
            string url = "https://" + api + "." + BaseUrl + "/docs/index.html";
            string docsPage = await Http.Client.GetStringRetry(url);

            foreach (string line in docsPage.Split(new char[] { '\r', '\n' }))
            {
                if (!line.StartsWith(SwaggerConfigurationPrefix)) continue;

                string configJson = line[SwaggerConfigurationPrefix.Length..^2];

                var config = JsonSerializer.Deserialize<SwaggerConfiguration>(configJson)!;

                return config.Versions;
            }

            throw new ApplicationException($"Could not find swagger configuration in {url}");
        }

        private async Task<string> GetDocs(string api, VersionInformation version)
        {
            string url = "https://" + api + "." + BaseUrl + "/docs/" + version.Url;
            string docs = await Http.Client.GetStringRetry(url);
            return docs;
        }

        private string ConstructPath(string api, VersionInformation version)
        {
            Directory.CreateDirectory(api);

            return Path.Combine(api, version.Version + ".json");
        }

        private bool AnyChanges(string file, string newContents)
        {
            if (!File.Exists(file))
                return false;

            string contents = File.ReadAllText(file);

            return contents != newContents;
        }

        private void CommitChanges(VersionInformation version, string file, string newContents)
        {
            Changes.Add(version.Name);

            File.WriteAllText(file, newContents);

            _Repository.Index.Add(file);
        }

        private void CheckForChanges(string api, VersionInformation version, string file, string newContents)
        {
            if (!AnyChanges(file, newContents))
                return;

            Console.WriteLine($"[{api}] New documentation ({version.Name})");

            CommitChanges(version, file, newContents);
        }

        public async Task Create()
        {
            while (_APIs.TryDequeue(out string? api))
            {
                if (api == null)
                    continue;

                Console.WriteLine($"[{api}] Fetching versions");

                var versions = await GetAllApiVersions(api);

                foreach (var version in versions)
                {
                    Console.WriteLine($"[{api}] {version.Version}");

                    string path = ConstructPath(api, version);

                    string docs = await GetDocs(api, version);

                    CheckForChanges(api, version, path, docs);
                }
            }
        }
    }
}
