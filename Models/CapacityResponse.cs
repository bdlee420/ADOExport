using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class CapacityResult : CapacityResponse
    {
        internal required IterationDto Iteration { get; set; }
        internal required Team Team { get; set; }
    }
    internal class CapacityResponse
    {
        [JsonProperty("teamMembers")]
        internal required List<TeamMemberCapacity> TeamMembers { get; set; }
    }

    internal class TeamMemberCapacity
    {
        [JsonProperty("teamMember")]
        internal required TeamMember TeamMember { get; set; }

        [JsonProperty("activities")]
        internal required List<Activity> Activities { get; set; }

        [JsonProperty("daysOff")]
        internal required List<DaysOff> DaysOff { get; set; }
    }

    internal class TeamMember
    {
        [JsonProperty("id")]
        internal required string Id { get; set; }

        [JsonProperty("displayName")]
        internal required string DisplayName { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is TeamMember other)
            {
                return Id == other.Id; // Assuming Id is the unique identifier
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0; // Use Id for hash code
        }
    }

    internal class TeamMemberDto
    {
        internal required string EmployeeAdoId { get; set; }

        [JsonProperty(nameof(Name))]
        internal required string Name { get; set; }

        [JsonProperty(nameof(IsLead))]
        internal bool IsLead { get; set; }

        [JsonProperty(nameof(IsFTE))]
        internal bool IsFTE { get; set; }

        [JsonProperty(nameof(Activity))]
        internal required string Activity { get; set; }

        [JsonProperty(nameof(TeamName))]
        internal required string TeamName { get; set; }

        [JsonProperty(nameof(BCE))]
        internal decimal BCE { get; set; } = 1.3M;

        [JsonProperty(nameof(Rating))]
        internal short Rating { get; set; }
    }

    internal class DaysOff
    {
        [JsonProperty("start")]
        internal DateTime Start { get; set; }

        [JsonProperty("end")]
        internal DateTime End { get; set; }
    }

    internal class Activity
    {
        [JsonProperty("capacityPerDay")]
        internal decimal CapacityPerDay { get; set; }

        [JsonProperty("name")]
        internal required string Name { get; set; }
    }

    internal class CapacityDto
    {
        internal int IterationAdoId { get; set; }
        internal required string IterationAdoIdentifier { get; set; }
        internal required string EmployeeAdoId { get; set; }
        internal required string TeamAdoId { get; set; }
        internal int Days { get; set; }
    }
}
