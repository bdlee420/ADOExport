using ADOExport.Models;

namespace ADOExport.Services
{
    internal static class WorkItemService
    {
        internal async static Task<WorkItemsResult> GetWorkItemsAsync(IEnumerable<Team> teams, List<IterationDto> iterations, string tag)
        {
            var workItems = await ADOService.GetWorkItemIds(teams, iterations);

            //Get all Task Ids that are tagged
            var taggedIds = await GetTaggedIdsAsync(teams, iterations, tag);

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
                    WorkItemType = w.Fields.WorkItemType,
                    IsCompliance = taggedIds.Contains(w.Id)
                }).ToList();

            Console.WriteLine($"Get WorkItems Count = {workItemDetailsDto.Count}");

            return new WorkItemsResult
            {
                WorkItemDetails = workItemDetails,
                WorkItemDetailsDtos = workItemDetailsDto
            };
        }

        private async static Task<HashSet<int>> GetTaggedIdsAsync(IEnumerable<Team> teams, List<IterationDto> iterations, string tag)
        {
            //Get all Task Ids that are tagged
            var workItemsTagged = await ADOService.GetWorkItemIdsByTag(teams, iterations, tag);
            var taggedIds = workItemsTagged
                .Where(w => w.Rel == "System.LinkTypes.Hierarchy-Forward" && w.Target != null && w.Target.Id > 0)
                .Select(w => w.Target.Id)
                .ToHashSet();
            return taggedIds;
        }
    }
}
