using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class QueryRequest
    {
        [JsonProperty("query")]
        internal string Query { get; set; }
    }
}
