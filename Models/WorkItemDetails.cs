using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class ErrorReponse
    {
        [JsonProperty("message")]
        internal string Message { get; set; }
    }

    internal class WorkItemReponse
    {
        [JsonProperty("count")]
        internal int Count { get; set; }

        [JsonProperty("value")]
        internal List<WorkItemDetails> Value { get; set; }
    }

    internal class WorkItemDetails
    {
        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("fields")]
        internal WorkItemDetailFields Fields { get; set; }

    }
    internal class WorkItemDetailFields
    {
        [JsonProperty("System.AreaId")]
        internal int AreaId { get; set; }

        [JsonProperty("System.AreaPath")]
        internal string AreaPath { get; set; }

        [JsonProperty("System.IterationPath")]
        internal string IterationPath { get; set; }

        [JsonProperty("System.IterationId")]
        internal int IterationId { get; set; }

        [JsonProperty("System.WorkItemType")]
        internal string WorkItemType { get; set; }

        [JsonProperty("System.AssignedTo")]
        internal TeamMember AssignedTo { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        internal decimal OriginalEstimate { get; set; }
    }

    internal class WorkItemDetailsDto
    {
        internal int WorkItemId { get; set; }

        internal int AreaAdoId { get; set; }

        internal string IterationPath { get; set; }

        internal int IterationId { get; set; }

        internal string WorkItemType { get; set; }

        internal string EmployeeAdoId { get; set; }

        internal decimal Estimate { get; set; }
    }

}
