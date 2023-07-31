using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs.Models
{
    internal class SwaggerConfiguration
    {
        [JsonPropertyName("urls")]
        public List<VersionInformation> Versions { get; set; } = null!;
    }
}
