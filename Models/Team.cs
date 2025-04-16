using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class TeamResponse
    {
        [JsonProperty("value")]
        internal required List<Team> Value { get; set; }
    }

    internal class Team
    {
        [JsonProperty("id")]
        internal required string Id { get; set; }

        [JsonProperty("name")]
        internal required string Name { get; set; }

        [JsonProperty("url")]
        internal required string Url { get; set; }

        internal required string AreaName { get; set; }

        internal required List<int> ReportIds { get; set; }
    }
}
