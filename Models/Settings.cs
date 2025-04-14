using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Settings
    {       
        [JsonProperty("authToken")]
        internal required string AuthToken { get; set; }

        [JsonProperty("personalAccessToken")]
        internal required string PersonalAccessToken { get; set; }

        [JsonProperty("projectId")]
        internal required string ProjectId { get; set; }

        [JsonProperty("projectName")]
        internal required string ProjectName { get; set; }

        [JsonProperty("projectNameUrl")]
        internal required string ProjectNameUrl { get; set; }

        [JsonProperty("connectionString")]
        internal required string ConnectionString { get; set; }

        [JsonProperty("rootUrl")]
        internal required string RootUrl { get; set; }
    }
}
