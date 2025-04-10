using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class Settings
    {
        [JsonProperty("authToken")]
        internal string AuthToken { get; set; }

        [JsonProperty("personalAccessToken")]
        internal string PersonalAccessToken { get; set; }

        [JsonProperty("projectId")]
        internal string ProjectId { get; set; }

        [JsonProperty("projectName")]
        internal string ProjectName { get; set; }

        [JsonProperty("projectNameUrl")]
        internal string ProjectNameUrl { get; set; }

        [JsonProperty("connectionString")]
        internal string ConnectionString { get; set; }

        [JsonProperty("rootUrl")]
        internal string RootUrl { get; set; }
    }
}
