using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models.Process;
using Newtonsoft.Json;

namespace ADOExport.Models
{
    internal class ErrorReponse
    {
        [JsonProperty("message")]
        internal required string Message { get; set; }
    }

    internal class WorkItemReponse
    {
        [JsonProperty("count")]
        internal int Count { get; set; }

        [JsonProperty("value")]
        internal required List<WorkItemDetails> Value { get; set; }
    }

    internal class WorkItemDetails
    {
        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("fields")]
        internal required WorkItemDetailFields Fields { get; set; }

        public override string? ToString()
        {
            return $"{Fields.WorkItemType}-{Id}-{Fields.AreaPath}";
        }
    }
    internal class WorkItemDetailFields
    {
        [JsonProperty("System.AreaId")]
        internal int AreaId { get; set; }

        [JsonProperty("System.AreaPath")]
        internal required string AreaPath { get; set; }

        [JsonProperty("System.IterationPath")]
        internal required string IterationPath { get; set; }

        [JsonProperty("System.State")]
        internal required string State { get; set; }

        [JsonProperty("System.IterationId")]
        internal int IterationId { get; set; }

        [JsonProperty("System.WorkItemType")]
        internal required string WorkItemType { get; set; }

        [JsonProperty("System.AssignedTo")]
        internal required TeamMember AssignedTo { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        internal decimal OriginalEstimate { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.RemainingWork")]
        internal decimal Remaining { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Activity")]
        internal string Activity { get; set; }
    }

    internal class WorkItemDetailsDto
    {
        internal int WorkItemId { get; set; }

        internal int AreaAdoId { get; set; }

        internal required string IterationPath { get; set; }

        internal int IterationId { get; set; }

        internal required string WorkItemType { get; set; }

        internal required string EmployeeAdoId { get; set; }

        internal decimal Estimate { get; set; }

        internal decimal Remaining { get; set; }

        internal string ParentType { get; set; }

        internal bool IsDone { get;set; }

        internal string Activity { get; set; }

        public override string? ToString()
        {
            return $"{WorkItemType}-{WorkItemId}-{IterationPath}";
        }
    }

}
