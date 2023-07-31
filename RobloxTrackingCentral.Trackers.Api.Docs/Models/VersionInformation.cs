using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs.Models
{
    internal class VersionInformation
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public string Version { get { return Url[(Url.LastIndexOf('/') + 1)..]; } }
    }
}
