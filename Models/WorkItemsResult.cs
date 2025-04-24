namespace ADOExport.Models
{
    internal class WorkItemsResult
    {
        internal List<WorkItemDetailsDto>? WorkItemDetailsDtos { get; set; }
        internal List<WorkItemDetails>? WorkItemDetails { get; set; }
        internal List<WorkItemTag>? WorkItemTags { get; set; }
    }
}
