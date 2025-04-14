using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class QueryRequest
    {
        [JsonProperty("query")]
        internal required string Query { get; set; }
    }
}
