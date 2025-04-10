namespace ADOExport.Models
{
    public class QueryResponse
    {
        public List<WorkItem> WorkItems { get; set; }
    }

    public class WorkItem
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }
}
