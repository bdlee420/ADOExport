using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Inputs
    {
        [JsonProperty("teams")]
        internal required List<TeamOverrides> Teams { get; set; }

        [JsonProperty("teamsOverrides")]
        internal required List<TeamOverrides> TeamsOverrides { get; set; }

        [JsonProperty("iterations")]
        internal required List<string> Iterations { get; set; }

        [JsonProperty("nameOverrides")]
        internal required List<NameOverrides> NameOverrides { get; set; }

        [JsonProperty("teamMembers")]
        internal required List<TeamMemberDto> TeamMembers { get; set; }
    }

    internal class TeamOverrides
    {
        [JsonProperty("teamName")]
        public required string TeamName { get; set; }

        [JsonProperty("areaName")]
        public required string AreaName { get; set; }

        [JsonProperty("reportIds")]
        public required List<int> ReportIds { get; set; }
    }

    internal class NameOverrides
    {
        public required string ADOName { get; set; }
        public required string NewName { get; set; }
    }
}