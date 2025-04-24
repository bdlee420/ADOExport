using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class IterationRequest
    {
        [JsonProperty("contributionIds")]
        internal required List<string> ContributionIds { get; set; }

        [JsonProperty("dataProviderContext")]
        internal required DataProviderContext DataProviderContext { get; set; }

    }

    internal class DataProviderContext
    {
        [JsonProperty("properties")]
        internal required DataProviderContextProperties Properties { get; set; }

    }

    internal class DataProviderContextProperties
    {
        [JsonProperty("sourcePage")]
        internal required SourcePage SourcePage { get; set; }

    }

    internal class SourcePage
    {
        [JsonProperty("routeId")]
        internal required string RouteId { get; set; }

        [JsonProperty("routeValues")]
        internal required RouteValues RouteValues { get; set; }
    }

    internal class RouteValues
    {
        [JsonProperty("project")]
        internal required string Project { get; set; }

        [JsonProperty("pivot")]
        internal required string Pivot { get; set; }

        [JsonProperty("teamName")]
        internal required string TeamName { get; set; }

        [JsonProperty("viewname")]
        internal required string Viewname { get; set; }

        [JsonProperty("iteration")]
        internal string? Iteration { get; set; }
    }
}
