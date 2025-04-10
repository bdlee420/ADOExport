using Newtonsoft.Json;

namespace ADOExport.Models
{
    public class IterationRequest
    {
        [JsonProperty("contributionIds")]
        public List<string> ContributionIds { get; set; }

        [JsonProperty("dataProviderContext")]
        public DataProviderContext DataProviderContext { get; set; }

    }

    public class DataProviderContext
    {
        [JsonProperty("properties")]
        public DataProviderContextProperties Properties { get; set; }

    }

    public class DataProviderContextProperties
    {
        [JsonProperty("sourcePage")]
        public SourcePage SourcePage { get; set; }

    }

    public class SourcePage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("routeId")]
        public string RouteId { get; set; }

        [JsonProperty("routeValues")]
        public RouteValues RouteValues { get; set; }

    }

    public class RouteValues
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("pivot")]
        public string Pivot { get; set; }

        [JsonProperty("teamName")]
        public string TeamName { get; set; }

        [JsonProperty("viewname")]
        public string Viewname { get; set; }

        [JsonProperty("iteration")]
        public string Iteration { get; set; }

    }
}
