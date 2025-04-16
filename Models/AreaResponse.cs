using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class AreaResponse
    {
        [JsonProperty("count")]
        internal int Count { get; set; }

        [JsonProperty("value")]
        internal required List<AreaRoot> Value { get; set; }
    }

    internal class AreaRoot
    {
        [JsonProperty("name")]
        internal required string Name { get; set; }

        [JsonProperty("children")]
        internal required List<Area> Children { get; set; }
    }

    internal class Area
    {
        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("identifier")]
        internal required string Identifier { get; set; }

        [JsonProperty("name")]
        internal required string Name { get; set; }

        [JsonProperty("path")]
        internal required string Path { get; set; }
    }
}
