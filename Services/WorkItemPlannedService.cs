using ADOExport.Models;

namespace ADOExport.Services
{
    internal class WorkItemPlannedService
    {
        internal async static Task<List<WorkItemPlannedData>> GetWorkItemPlannedData(IEnumerable<Team> selectedTeams, List<IterationDto> iterationsDto)
        {
            var all_work_items = new List<WorkItemDetails>();

            var completion_data_start = new List<CompletionDataKey>();
            var completion_data_end = new List<CompletionDataKey>();
            var completion_data_end_deleted = new List<CompletionDataKey>();

            var completion_data_start_hash = new HashSet<CompletionDataKey>();
            var completion_data_end_hash = new HashSet<CompletionDataKey>();
            var completion_data_end_deleted_hash = new HashSet<CompletionDataKey>();

            var all_ids = new List<int>();
            var ids_exists_at_end = new List<int>();

            foreach (var iteration in iterationsDto)
            {
                var start_date = iteration.StartDate.AddDays(-1);
                var result_start = await ADOService.GetWorkItemIdsAsOf_Start(start_date, iteration, selectedTeams);
                var start_ids = result_start.Select(s => s.Id).ToList();
                if (start_ids.Count == 0)
                    continue;

                var end_date = iteration.EndDate.AddDays(4);

                all_ids.AddRange(start_ids);
                var result_end = await ADOService.GetWorkItemIdsAsOf_End(end_date, start_ids, iteration);
                var result_end_no_filter = await ADOService.GetWorkItemIdsAsOf_End_NoFilter(end_date, start_ids, iteration);

                var ids_not_deleted = result_end_no_filter.Select(r => r.Id).ToList();
                var ids_deleted = start_ids.Except(ids_not_deleted).ToHashSet();
                var work_items_deleted = result_start.Where(r => ids_deleted.Contains(r.Id));

                completion_data_start.AddRange(result_start.Select(r => new CompletionDataKey
                {
                    IterationAdoId = iteration.Id,
                    WorkItemId = r.Id
                }));

                completion_data_end.AddRange(result_end.Select(r => new CompletionDataKey
                {
                    IterationAdoId = iteration.Id,
                    WorkItemId = r.Id
                }));

                completion_data_end_deleted.AddRange(work_items_deleted.Select(r => new CompletionDataKey
                {
                    IterationAdoId = iteration.Id,
                    WorkItemId = r.Id
                }));

                //Get WorkItem Details based on StartDate (AssignedTo, WorkItemType, Area)
                all_work_items.AddRange(await ADOService.GetWorkItemDetails(ids_not_deleted, end_date));
                all_work_items.AddRange(await ADOService.GetWorkItemDetails(ids_deleted.ToList(), start_date));
            }

            completion_data_end_deleted_hash = completion_data_end_deleted.ToHashSet();
            completion_data_end_hash = completion_data_end.ToHashSet();

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
                    IsDeleted = completion_data_end_deleted_hash.Contains(key),
                    IsDone = completion_data_end_hash.Contains(key)
                };

                return data;
            }).ToList();

            //completion_data_end_deleted.Select(w => new WorkItemPlannedData
            //{
            //    AreaAdoId = 0,
            //    IsDeleted = true,
            //    IsDone = false,
            //    IterationId = w.IterationAdoId,
            //    WorkItemId = w.WorkItemId
            //});

            Console.WriteLine($"Get WorkItemPlannedData Count = {workItemPlannedData.Count}");

            return workItemPlannedData;
        }
    }
}
