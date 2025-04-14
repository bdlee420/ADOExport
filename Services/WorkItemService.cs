using ADOExport.Models;

namespace ADOExport.Services
{
    internal static class WorkItemService
    {       
        internal async static Task<WorkItemsResult> GetWorkItemsAsync(List<IterationDto> iterations)
        {
            var workItems = await ADOService.GetWorkItemIds(iterations);
            var workItemIds = workItems.Select(w => w.Id).ToList();
            var workItemDetails = await ADOService.GetWorkItemDetails(workItemIds);

            var workItemDetailsDto = workItemDetails
                .Where(w => w.Fields.AssignedTo != null)
                .Select(w => new WorkItemDetailsDto
                {
                    WorkItemId = w.Id,
                    EmployeeAdoId = w.Fields.AssignedTo.Id,
                    Estimate = w.Fields.OriginalEstimate,
                    IterationPath = w.Fields.IterationPath,
                    IterationId = w.Fields.IterationId,
                    AreaAdoId = w.Fields.AreaId,
                    WorkItemType = w.Fields.WorkItemType
                }).ToList();

            return new WorkItemsResult
            {
                WorkItemDetails = workItemDetails,
                WorkItemDetailsDtos = workItemDetailsDto
            };
        }
    }
}
