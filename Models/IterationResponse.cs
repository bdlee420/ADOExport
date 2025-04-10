using Newtonsoft.Json;

namespace ADOExport.Models
{
    public class IterationResponse
    {
        public DataProvider DataProviders { get; set; }
    }

    public class DataProvider
    {
        [JsonProperty("ms.vss-work-web.new-team-wit-settings-data-provider")]
        public DataProviderMeta DataProviderMeta { get; set; }
    }

    public class DataProviderMeta
    {
        public List<Iteration> PreviousIterations { get; set; }
        public Iteration CurrentIteration { get; set; }
    }

    public class Iteration
    {
        public int NodeId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string FriendlyPath { get; set; }
    }

    public class IterationDto
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string YearQuarter { get; set; }
    }
}
