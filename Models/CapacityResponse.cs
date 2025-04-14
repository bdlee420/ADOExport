using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class CapacityResult : CapacityResponse
    {
        internal IterationDto Iteration { get; set; }
        internal Team Team { get; set; }
    }
    internal class CapacityResponse
    {
        [JsonProperty("teamMembers")]
        internal List<TeamMemberCapacity> TeamMembers { get; set; }
    }

    internal class TeamMemberCapacity
    {
        [JsonProperty("teamMember")]
        internal TeamMember TeamMember { get; set; }

        [JsonProperty("activities")]
        internal List<Activity> Activities { get; set; }

        [JsonProperty("daysOff")]
        internal List<DaysOff> DaysOff { get; set; }
    }

    internal class TeamMember
    {
        [JsonProperty("id")]
        internal string Id { get; set; }

        [JsonProperty("displayName")]
        internal string DisplayName { get; set; }

        public override bool Equals(object obj)
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
        internal string EmployeeAdoId { get; set; }

        [JsonProperty("Name")]
        internal string Name { get; set; }

        [JsonProperty("IsLead")]
        internal bool IsLead { get; set; }

        [JsonProperty("IsFTE")]
        internal bool IsFTE { get; set; }

        [JsonProperty("Activity")]
        internal string Activity { get; set; }

        [JsonProperty("TeamName")]
        internal string TeamName { get; set; }

        [JsonProperty("BCE")]
        internal decimal BCE { get; set; } = 1.3M;

        [JsonProperty("Rating")]
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
        internal string Name { get; set; }
    }

    internal class CapacityDto
    {
        internal int IterationAdoId { get; set; }
        internal string IterationAdoIdentifier { get; set; }
        internal string EmployeeAdoId { get; set; }
        internal string TeamAdoId { get; set; }
        internal int Days { get; set; }
    }
}
