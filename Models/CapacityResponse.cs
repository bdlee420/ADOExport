namespace ADOExport.Models
{
    public class CapacityResult : CapacityResponse
    {
        public IterationDto Iteration { get; set; }
        public Team Team { get; set; }
    }
    public class CapacityResponse
    {
        public List<TeamMemberCapacity> TeamMembers { get; set; }
    }

    public class TeamMemberCapacity
    {
        public TeamMember TeamMember { get; set; }

        public List<Activity> Activities { get; set; }

        public List<DaysOff> DaysOff { get; set; }
    }

    public class TeamMember
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

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

    public class TeamMemberDto
    {
        public string EmployeeAdoId { get; set; }
        public string Name { get; set; }
        public bool IsLead { get; set; }
        public bool IsFTE { get; set; }
        public string Activity { get; set; }
        public string TeamName { get; set; }
        public decimal BCE { get; set; } = 1.3M;
        public short Rating { get; set; }
    }

    public class DaysOff
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class Activity
    {
        public decimal CapacityPerDay { get; set; }
        public string Name { get; set; }
    }

    public class CapacityDto
    {
        public int IterationAdoId { get; set; }
        public string IterationAdoIdentifier { get; set; }
        public string EmployeeAdoId { get; set; }
        public string TeamAdoId { get; set; }
        public int Days { get; set; }
    }
}
