using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Inputs
    {
        [JsonProperty("teams")]
        internal List<string> Teams { get; set; }

        [JsonProperty("iterations")]
        internal List<string> Iterations { get; set; }

        [JsonProperty("nameOverrides")]
        internal List<NameOverrides> NameOverrides { get; set; }

        [JsonProperty("teamMembers")]
        internal List<TeamMemberDto> TeamMembers { get; set; }
    }

    internal class NameOverrides
    {
        public string ADOName { get; set; }
        public string NewName { get; set; }
    }
}