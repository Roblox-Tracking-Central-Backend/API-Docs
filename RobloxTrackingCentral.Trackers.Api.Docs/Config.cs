using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs
{
    internal class Config
    {
        public static Config Default { get; }

        public int Workers { get; set; } = 5;

        static Config()
        {
            try
            {
                Console.WriteLine("Fetching config from " + Repositories.Backend);
                string configJsonStr = Http.Client.GetStringRetry("https://raw.githubusercontent.com/" + Repositories.Backend + "/main/Config.json").Result;
                Default = JsonSerializer.Deserialize<Config>(configJsonStr)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch config: {ex}");
                Console.WriteLine("Using default configuration");
                Default = new Config();
            }
        }
    }
}
