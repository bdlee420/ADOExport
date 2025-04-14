using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Inputs
    {
        [JsonProperty("teams")]
        internal List<TeamOverrides> Teams { get; set; }

        [JsonProperty("iterations")]
        internal List<string> Iterations { get; set; }

        [JsonProperty("nameOverrides")]
        internal List<NameOverrides> NameOverrides { get; set; }

        [JsonProperty("teamMembers")]
        internal List<TeamMemberDto> TeamMembers { get; set; }
    }

    internal class TeamOverrides
    {
        [JsonProperty("teamName")]
        public string TeamName { get; set; }

        [JsonProperty("areaName")]
        public string AreaName { get; set; }
    }

    internal class NameOverrides
    {
        public string ADOName { get; set; }
        public string NewName { get; set; }
    }
}