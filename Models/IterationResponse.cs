using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class IterationResponse
    {
        internal DataProvider DataProviders { get; set; }
    }

    internal class DataProvider
    {
        [JsonProperty("ms.vss-work-web.new-team-wit-settings-data-provider")]
        internal DataProviderMeta DataProviderMeta { get; set; }
    }

    internal class DataProviderMeta
    {
        internal List<Iteration> PreviousIterations { get; set; }
        internal Iteration CurrentIteration { get; set; }
    }

    internal class Iteration
    {
        internal int NodeId { get; set; }
        internal string Id { get; set; }
        internal string Name { get; set; }
        internal DateTime StartDate { get; set; }
        internal DateTime FinishDate { get; set; }
        internal string FriendlyPath { get; set; }
    }

    internal class IterationDto
    {
        internal int Id { get; set; }
        internal string Identifier { get; set; }
        internal string Name { get; set; }
        internal string Path { get; set; }
        internal DateTime StartDate { get; set; }
        internal DateTime EndDate { get; set; }
        internal string YearQuarter { get; set; }
    }
}
