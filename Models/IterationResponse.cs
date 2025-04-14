using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class IterationResponse
    {
        [JsonProperty("dataProviders")]
        internal required DataProvider DataProviders { get; set; }
    }

    internal class DataProvider
    {
        [JsonProperty("ms.vss-work-web.new-team-wit-settings-data-provider")]
        internal required DataProviderMeta DataProviderMeta { get; set; }
    }

    internal class DataProviderMeta
    {
        [JsonProperty("previousIterations")]
        internal required List<Iteration> PreviousIterations { get; set; }

        [JsonProperty("currentIteration")]
        internal required Iteration CurrentIteration { get; set; }
    }

    internal class Iteration
    {
        [JsonProperty("nodeId")]
        internal int NodeId { get; set; }

        [JsonProperty("id")]
        internal required string Id { get; set; }

        [JsonProperty("name")]
        internal required string Name { get; set; }

        [JsonProperty("startDate")]
        internal DateTime StartDate { get; set; }

        [JsonProperty("finishDate")]
        internal DateTime FinishDate { get; set; }

        [JsonProperty("friendlyPath")]
        internal required string FriendlyPath { get; set; }
    }

    internal class IterationDto
    {
        internal int Id { get; set; }
        internal required string Identifier { get; set; }
        internal required string Name { get; set; }
        internal required string Path { get; set; }
        internal DateTime StartDate { get; set; }
        internal DateTime EndDate { get; set; }
        internal required string YearQuarter { get; set; }
    }
}
