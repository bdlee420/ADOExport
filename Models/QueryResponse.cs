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
    }
}
