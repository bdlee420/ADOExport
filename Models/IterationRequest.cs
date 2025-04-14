using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class IterationRequest
    {
        [JsonProperty("contributionIds")]
        internal List<string> ContributionIds { get; set; }

        [JsonProperty("dataProviderContext")]
        internal DataProviderContext DataProviderContext { get; set; }

    }

    internal class DataProviderContext
    {
        [JsonProperty("properties")]
        internal DataProviderContextProperties Properties { get; set; }

    }

    internal class DataProviderContextProperties
    {
        [JsonProperty("sourcePage")]
        internal SourcePage SourcePage { get; set; }

    }

    internal class SourcePage
    {
        [JsonProperty("url")]
        internal string Url { get; set; }

        [JsonProperty("routeId")]
        internal string RouteId { get; set; }

        [JsonProperty("routeValues")]
        internal RouteValues RouteValues { get; set; }

    }

    internal class RouteValues
    {
        [JsonProperty("project")]
        internal string Project { get; set; }

        [JsonProperty("pivot")]
        internal string Pivot { get; set; }

        [JsonProperty("teamName")]
        internal string TeamName { get; set; }

        [JsonProperty("viewname")]
        internal string Viewname { get; set; }

        [JsonProperty("iteration")]
        internal string Iteration { get; set; }

    }
}
