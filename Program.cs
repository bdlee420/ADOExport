using System.Diagnostics;
using ADOExport.Common;
using ADOExport.Data;
using ADOExport.Models;
using ADOExport.Services;

namespace ADOExport
{
    internal class Program
    {
        static async Task Main()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms - Setup Inputs");
            await SettingsService.SetCurrentSettingsAsync();
            if (SettingsService.CurrentSettings == null)
            {
                Console.WriteLine("No settings!");
                return;
            }

            await SettingsService.SetInputsAsync();
            if (SettingsService.CurrentInputs == null || SettingsService.CurrentInputs.Teams.Count == 0)
            {
                Console.WriteLine("No inputs!");
                return;
            }

            //Get Meta Data
            var areas = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Areas", () => AreaService.GetAreasAsync());
            var selectedTeams = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Teams", () => TeamsService.GetTeamsAsync(SettingsService.CurrentInputs.Teams));
            var iterationsDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Iterations", () => IterationsService.GetIterationsAsync(SettingsService.CurrentInputs.Iterations));

            //Report - Planned Done
            var selected_teams_report_1 = selectedTeams.Where(s => s.ReportIds.Contains((int)Reports.PlannedDone));
            var workItemPlannedData = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get WorkItemPlannedData", () => WorkItemPlannedService.GetWorkItemPlannedData(selected_teams_report_1, iterationsDto));

            //Report - Employee Reporting
            var selected_teams_report_2 = selectedTeams.Where(s => s.ReportIds.Contains((int)Reports.EmployeeReporting));
            var capacitiesDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Capacities", () => CapacitiesService.GetCapacitiesAsync(selected_teams_report_2, iterationsDto));
            var workItemResult = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get WorkItems", () => WorkItemService.GetWorkItemsAsync(selected_teams_report_2, iterationsDto, SettingsService.CurrentInputs.Tags));
            var employeesDto = ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Employees", () => EmployeeService.GetEmployees(workItemResult.WorkItemDetails));

            //Store Data in SQL
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Areas", () => SqlDataProvider.AddAreas(areas));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Teams", () => SqlDataProvider.AddTeams(selectedTeams));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItems Planned/Done", () => SqlDataProvider.AddUpdateWorkItemsPlannedDone(workItemPlannedData)); 
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItems", () => SqlDataProvider.AddUpdateWorkItems(workItemResult.WorkItemDetailsDtos));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Capacities", () => SqlDataProvider.AddCapacities(capacitiesDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Employees", () => SqlDataProvider.AddEmployees(employeesDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Iterations", () => SqlDataProvider.AddIterations(iterationsDto));            

            Console.WriteLine($"Total Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");
        }

        enum Reports
        {
            PlannedDone = 1,
            EmployeeReporting = 2,
            Compliance = 3
        }
    }
}
