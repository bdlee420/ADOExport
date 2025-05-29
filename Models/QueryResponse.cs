using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class QueryResponse
    {
        [JsonProperty("workItems")]
        internal required List<WorkItem> WorkItems { get; set; }
    }

    internal class WorkItem
    {
        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("url")]
        internal required string Url { get; set; }

        public override string? ToString()
        {
            return Id.ToString();
        }
    }

    internal class QueryChildrenResponse
    {
        [JsonProperty("workItemRelations")]
        internal required List<WorkItemChildren> WorkItemRelations { get; set; }
    }

    internal class WorkItemChildren
    {
        [JsonProperty("target")]
        internal WorkItem Target { get; set; }

        [JsonProperty("rel")]
        internal string? Rel { get; set; }
    }
}
