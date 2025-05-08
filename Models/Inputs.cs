using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Inputs
    {
        [JsonProperty("runSettings")]
        internal required RunSettings RunSettings { get; set; }

        [JsonProperty("tags")]
        internal required List<string> Tags { get; set; }

        [JsonProperty("project_tags")]
        internal required List<string> ProjectTags { get; set; }

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

    internal class RunSettings
    {
        [JsonProperty("loadAreas")]
        public required bool LoadAreas { get; set; }

        [JsonProperty("loadTeams")]
        public required bool LoadTeams { get; set; }

        [JsonProperty("loadCapacities")]
        public required bool LoadCapacities { get; set; }

        [JsonProperty("loadIterations")]
        public required bool LoadIterations { get; set; }

        [JsonProperty("loadEmployees")]
        public required bool LoadEmployees { get; set; }

        [JsonProperty("loadPlannedDone")]
        public required bool LoadPlannedDone { get; set; }

        [JsonProperty("loadWorkItems")]
        public required bool LoadWorkItems { get; set; }

        [JsonProperty("loadTags")]
        public required bool LoadTags { get; set; }

        [JsonProperty("loadProjectTags")]
        public required bool LoadProjectTags { get; set; }
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