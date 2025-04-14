using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class TeamResponse
    {
        [JsonProperty("value")]
        internal List<Team> Value { get; set; }
    }

    internal class Team
    {
        internal string Id { get; set; }
        internal string Name { get; set; }
        internal string Url { get; set; }
    }
}
