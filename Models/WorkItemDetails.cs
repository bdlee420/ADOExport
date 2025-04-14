using Newtonsoft.Json;

namespace ADOExport.Models
{
    public class ErrorReponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class WorkItemReponse
    {
        public int Count { get; set; }
        public List<WorkItemDetails> Value { get; set; }
    }

    public class WorkItemDetails
    {
        public int Id { get; set; }
        public WorkItemDetailFields Fields { get; set; }

    }
    public class WorkItemDetailFields
    {
        [JsonProperty("System.AreaId")]
        public int AreaId { get; set; }

        [JsonProperty("System.AreaPath")]
        public string AreaPath { get; set; }

        [JsonProperty("System.IterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty("System.IterationId")]
        public int IterationId { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }

        [JsonProperty("System.AssignedTo")]
        public TeamMember AssignedTo { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        public decimal OriginalEstimate { get; set; }
    }

    public class WorkItemDetailsDto
    {
        public int WorkItemId { get; set; }

        public int AreaAdoId { get; set; }

        public string IterationPath { get; set; }

        public int IterationId { get; set; }

        public string WorkItemType { get; set; }

        public string EmployeeAdoId { get; set; }

        public decimal Estimate { get; set; }
    }

}
