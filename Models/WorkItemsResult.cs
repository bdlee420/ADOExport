namespace ADOExport.Models
{
    internal class WorkItemsResult
    {
        internal required List<WorkItemDetailsDto> WorkItemDetailsDtos { get; set; }
        internal required List<WorkItemDetails> WorkItemDetails { get; set; }
    }
}
