namespace ADOExport.Models
{
    internal class WorkItemPlannedData
    {
        internal int WorkItemId { get; set; }
        internal string? EmployeeAdoId { get; set; }
        internal int IterationId { get; set; }
        internal int AreaAdoId { get; set; }
        internal bool IsDone { get; set; }
        internal bool IsPlanned { get; set; }
        internal bool IsDeleted { get; set; }
        internal bool IsRemovedFromSprint { get; set; }

        public override string? ToString()
        {
            return $"{WorkItemId}-{IterationId}-{AreaAdoId}-{EmployeeAdoId}";
        }
    }   

    internal class CompletionDataKey
    {
        internal int WorkItemId { get; set; }
        internal int IterationAdoId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is CompletionDataKey other)
            {
                return WorkItemId == other.WorkItemId && IterationAdoId == other.IterationAdoId;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WorkItemId, IterationAdoId);
        }

        public override string? ToString()
        {
            return $"{WorkItemId}-{IterationAdoId}";
        }
    }
}
