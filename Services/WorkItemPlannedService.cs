using ADOExport.Models;
using Microsoft.VisualStudio.Services.Common;

namespace ADOExport.Services
{
    internal class WorkItemPlannedService
    {
        internal async static Task<List<WorkItemPlannedData>> GetWorkItemPlannedData(IEnumerable<Team> selectedTeams, List<IterationDto> iterationsDto)
        {
            var all_work_items = new List<WorkItemDetails>();
            var all_done_workitems_end = new HashSet<CompletionDataKey>();
            var all_deleted_workitems_end = new HashSet<CompletionDataKey>();
            var all_unplanned_workitems = new HashSet<CompletionDataKey>();

            foreach (var iteration in iterationsDto)
            {
                //Setup Snapshot Dates

                var is_3_week_sprint = iteration.StartDate < new DateTime(2025, 4, 20);
                var start_date = is_3_week_sprint ? iteration.StartDate.AddDays(-1) : iteration.StartDate;
                var end_date = is_3_week_sprint ? iteration.EndDate.AddDays(4) : iteration.EndDate.AddDays(1);

                //Get all planned workitems. This is all tasks and defects that exist in a non closed state at the beginning of the sprint
                var planned_workitems_start = await ADOService.GetNotDoneWorkItemIdsAsOf_Start(start_date, iteration, selectedTeams);
                var planned_workitems_ids = planned_workitems_start.Select(s => s.Id).ToHashSet();

                //Get all workitems that are closed as of the end date for this iteration
                var iteration_done_workitems_end = await ADOService.GetDoneWorkItemIdsAsOf_End(end_date, iteration, selectedTeams);

                //Get all workitems tas of the end date for this iteration
                var iteration_all_workitems_end = await ADOService.GetAllWorkItemIdsAsOf_End(end_date, iteration, selectedTeams);

                //Get rid of WorkItems that were done before the Current Sprint but for some reason still in the Current Sprint
                var done_workitems_ids = iteration_done_workitems_end.Select(w => w.Id);
                var iteration_done_workitems_start = await ADOService.GetDoneWorkItemIdsAsOf_Start(start_date, iteration, done_workitems_ids);
                var ids_to_remove = iteration_done_workitems_start.Select(w => w.Id);
                iteration_all_workitems_end.RemoveAll(w => ids_to_remove.Contains(w.Id));
                iteration_done_workitems_end.RemoveAll(w => ids_to_remove.Contains(w.Id));

                var ids_not_deleted = iteration_all_workitems_end.Select(r => r.Id).ToList();
                var ids_deleted = planned_workitems_ids.Except(ids_not_deleted).ToHashSet();
                var iteration_deleted_workitems_end = planned_workitems_start.Where(r => ids_deleted.Contains(r.Id));

                all_unplanned_workitems.AddRange(iteration_all_workitems_end
                    .Where(r => !planned_workitems_ids.Contains(r.Id))
                    .Select(r => new CompletionDataKey
                    {
                        IterationAdoId = iteration.Id,
                        WorkItemId = r.Id
                    }));

                all_done_workitems_end.AddRange(iteration_done_workitems_end.Select(r => new CompletionDataKey
                {
                    IterationAdoId = iteration.Id,
                    WorkItemId = r.Id
                }));

                all_deleted_workitems_end.AddRange(iteration_deleted_workitems_end.Select(r => new CompletionDataKey
                {
                    IterationAdoId = iteration.Id,
                    WorkItemId = r.Id
                }));

                //Get WorkItem Details based on StartDate (AssignedTo, WorkItemType, Area)
                all_work_items.AddRange(await ADOService.GetWorkItemDetails(ids_not_deleted, end_date));
                all_work_items.AddRange(await ADOService.GetWorkItemDetails(ids_deleted.ToList(), start_date));
            }

            var removedDict = await WorkItemService.GetTaggedIdsAsync(selectedTeams, iterationsDto, ["Approved Removal"]);
            var removed = removedDict
                .Where(w => w.Tag == "Approved Removal")
                .Select(w => w.WorkItemId)
                .ToHashSet();

            var workItemPlannedData = all_work_items.Select(w =>
            {
                var key = new CompletionDataKey
                {
                    IterationAdoId = w.Fields.IterationId,
                    WorkItemId = w.Id
                };

                var data = new WorkItemPlannedData
                {
                    WorkItemId = w.Id,
                    EmployeeAdoId = w.Fields.AssignedTo?.Id,
                    AreaAdoId = w.Fields.AreaId,
                    IterationId = w.Fields.IterationId,
                    IsDeleted = all_deleted_workitems_end.Contains(key) || w.Fields.State == "Removed",
                    IsPlanned = !all_unplanned_workitems.Contains(key),
                    IsDone = all_done_workitems_end.Contains(key),
                    IsRemovedFromSprint = !all_done_workitems_end.Contains(key) && removed.Contains(key.WorkItemId)
                };

                return data;
            }).ToList();

            Console.WriteLine($"Get WorkItemPlannedData Count = {workItemPlannedData.Count}");

            return workItemPlannedData;
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
