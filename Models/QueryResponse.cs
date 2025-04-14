using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class QueryResponse
    {
        [JsonProperty("workItems")]
        internal List<WorkItem> WorkItems { get; set; }
    }

    internal class WorkItem
    {
        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("url")]
        internal string Url { get; set; }
    }
}
