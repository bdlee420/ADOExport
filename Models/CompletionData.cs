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

    internal class WorkItemPlannedDataComparer : IEqualityComparer<WorkItemPlannedData>
    {
        public bool Equals(WorkItemPlannedData x, WorkItemPlannedData y)
        {
            // Define equality based on WorkItemId
            return x.WorkItemId == y.WorkItemId && 
                x.EmployeeAdoId == y.EmployeeAdoId && 
                x.IsPlanned == y.IsPlanned && 
                x.IsDeleted == y.IsDeleted &&
                x.IsDone == y.IsDone &&
                x.IterationId == y.IterationId &&
                x.AreaAdoId == y.AreaAdoId &&
                x.IsRemovedFromSprint == y.IsRemovedFromSprint;
        }

        public int GetHashCode(WorkItemPlannedData obj)
        {
            // Use WorkItemId for hash code
            return obj.WorkItemId.GetHashCode();
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
