using ADOExport.Models;

namespace ADOExport.Services
{
    internal static class WorkItemService
    {
        internal async static Task<WorkItemsResult> GetWorkItemsAsync(IEnumerable<Team> teams, List<IterationDto> iterations, List<string> tags, bool loadTags)
        {
            var workItems = await ADOService.GetWorkItemIds(teams, iterations);

            //Get all Task Ids that are tagged
            var workItemTags = loadTags ? await GetTaggedIdsAsync(teams, iterations, tags) : null;

            //Get WorkItemIds by Type
            var bugs = GetHashSet(await ADOService.GetWorkItemIds(teams, iterations, "Bug"));
            var userStories = GetHashSet(await ADOService.GetWorkItemIds(teams, iterations, "User Story"));
            var deployments = GetHashSet(await ADOService.GetWorkItemIds(teams, iterations, "Deployment"));

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
                    IsDone = true,
                    Activity = w.Fields.Activity,
                    ParentType = GetParentType(w.Id, bugs, userStories, deployments)
                }).ToList();

            Console.WriteLine($"Get WorkItems Count = {workItemDetailsDto.Count}");

            return new WorkItemsResult
            {
                WorkItemDetails = workItemDetails,
                WorkItemDetailsDtos = workItemDetailsDto,
                WorkItemTags = workItemTags
            };
        }

        internal async static Task<WorkItemsResult> GetProjectWorkItemsAsync(IEnumerable<Team> teams, List<string> tags)
        {
            //Get all Task Ids that are tagged
            var workItemTags = await GetTaggedIdsAsync(teams, tags);
            var workItemIds = workItemTags.Select(w => w.WorkItemId).ToList();
            var workItemDetails = await ADOService.GetWorkItemDetails(workItemIds);

            var workItemDetailsDto = workItemDetails
                .Where(w => w.Fields.WorkItemType == "Task" || w.Fields.WorkItemType == "Defect")
                .Select(w => new WorkItemDetailsDto
                {
                    WorkItemId = w.Id,
                    EmployeeAdoId = w.Fields.AssignedTo?.Id ?? "-1",
                    Estimate = w.Fields.OriginalEstimate,
                    Remaining = w.Fields.Remaining,
                    IterationPath = w.Fields.IterationPath,
                    IterationId = w.Fields.IterationId,
                    AreaAdoId = w.Fields.AreaId,
                    WorkItemType = w.Fields.WorkItemType,
                    IsDone = false,
                    Activity = w.Fields.Activity
                    //ParentType = GetParentType(w.Id, bugs, userStories, deployments)
                }).ToList();

            Console.WriteLine($"Get Project Workitem Tagged Count = {workItemDetailsDto.Count}");

            return new WorkItemsResult
            {
                WorkItemDetails = workItemDetails,
                WorkItemDetailsDtos = workItemDetailsDto,
                WorkItemTags = workItemTags
            };
        }

        private static string GetParentType(int workItemId, HashSet<int> bugs, HashSet<int> stories, HashSet<int> deployments)
        {
            if (bugs.Contains(workItemId)) { return "Bug"; }
            else if (stories.Contains(workItemId)) { return "User Story"; }
            else if (deployments.Contains(workItemId)) { return "Deployment"; }
            else { return string.Empty; };
        }

        private static HashSet<int> GetHashSet(List<WorkItemChildren> workItems)
        {
            return workItems
                .Where(w => w.Rel == "System.LinkTypes.Hierarchy-Forward" && w.Target != null && w.Target.Id > 0)
                .Select(w => w.Target.Id)
                .ToHashSet();
        }

        private async static Task<List<WorkItemTag>> GetTaggedIdsAsync(IEnumerable<Team> teams, List<IterationDto> iterations, List<string> tags)
        {
            var workItemTags = new List<WorkItemTag>();

            //Get all Task Ids that are tagged
            var workItemsTagged = await ADOService.GetWorkItemIdsByTag(teams, iterations, tags);

            foreach (var value in workItemsTagged)
            {
                foreach (var workItemId in value.Value
                    .Where(w => w.Rel == "System.LinkTypes.Hierarchy-Forward" && w.Target != null && w.Target.Id > 0)
                    .Select(w => w.Target.Id))
                {
                    workItemTags.Add(new WorkItemTag { Tag = value.Key, WorkItemId = workItemId });
                }
            }

            return workItemTags;
        }

        private async static Task<List<WorkItemTag>> GetTaggedIdsAsync(IEnumerable<Team> teams, List<string> tags)
        {
            var workItemTags = new List<WorkItemTag>();

            //Get all Task Ids that are tagged
            var workItemsTagged = await ADOService.GetWorkItemIdsByTag(teams, tags);

            foreach (var value in workItemsTagged)
            {
                foreach (var workItemId in value.Value
                    .Where(w => w.Rel == "System.LinkTypes.Hierarchy-Forward" && w.Target != null && w.Target.Id > 0)
                    .Select(w => w.Target.Id))
                {
                    workItemTags.Add(new WorkItemTag { Tag = value.Key, WorkItemId = workItemId });
                }
            }

            return workItemTags;
        }
    }
}
