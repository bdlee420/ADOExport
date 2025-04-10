using Newtonsoft.Json;

namespace ADOExport.Models
{
    public class QueryRequest
    {
        [JsonProperty("query")]
        public string Query { get; set; }
    }
}
