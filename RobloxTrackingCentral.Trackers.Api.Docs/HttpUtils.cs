using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs
{
    internal static class HttpClientEx
    {
        private const int MaxRetries = 5;

        public static async Task<string> GetStringRetry(this HttpClient httpClient, string url)
        {
            for (int i = 1; i <= MaxRetries; i++)
            {
                try
                {
                    var message = await httpClient.GetAsync(url);
                    message.EnsureSuccessStatusCode();
                    var content = await message.Content.ReadAsStringAsync();
                    return content;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch {url} ({i}): {ex}");
                }
            }

            throw new HttpRequestException($"Could not fetch {url}");
        }
    }
}
