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
                Console.WriteLine("Must have valid appsettings.override.json file!");
                return;
            }

            await SettingsService.SetInputsAsync();
            if (SettingsService.CurrentInputs == null)
            {
                Console.WriteLine("Must have valid inputs.override.json file!");
                return;
            }

            var selectedTeams = new List<Team>();
            var selected_teams_planned = Enumerable.Empty<Team>();
            var selected_teams_employee_reporting = Enumerable.Empty<Team>();
            var selected_teams_tags = Enumerable.Empty<Team>();
            var iterationsDto = new List<IterationDto>();
            var workItemResult = new WorkItemsResult();

            bool runPlannedDone = SettingsService.CurrentInputs.RunSettings.LoadPlannedDone;
            bool runEmployees = SettingsService.CurrentInputs.RunSettings.LoadEmployees;
            bool runAreas = SettingsService.CurrentInputs.RunSettings.LoadAreas;
            bool runCapacities = SettingsService.CurrentInputs.RunSettings.LoadCapacities;
            bool runTags = SettingsService.CurrentInputs.RunSettings.LoadTags;

            bool runWorkItems = SettingsService.CurrentInputs.RunSettings.LoadWorkItems
                || SettingsService.CurrentInputs.RunSettings.LoadEmployees
                || SettingsService.CurrentInputs.RunSettings.LoadTags;

            bool runIterations = SettingsService.CurrentInputs.RunSettings.LoadIterations
                || SettingsService.CurrentInputs.RunSettings.LoadWorkItems
                || SettingsService.CurrentInputs.RunSettings.LoadCapacities
                || SettingsService.CurrentInputs.RunSettings.LoadPlannedDone
                || SettingsService.CurrentInputs.RunSettings.LoadTags;

            bool runTeams = SettingsService.CurrentInputs.RunSettings.LoadTeams
                || SettingsService.CurrentInputs.RunSettings.LoadPlannedDone
                || SettingsService.CurrentInputs.RunSettings.LoadCapacities
                || SettingsService.CurrentInputs.RunSettings.LoadWorkItems
                || SettingsService.CurrentInputs.RunSettings.LoadEmployees
                || SettingsService.CurrentInputs.RunSettings.LoadTags;

            if (runIterations)
            {
                iterationsDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Iterations", () => IterationsService.GetIterationsAsync(SettingsService.CurrentInputs.Iterations));
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Iterations", () => SqlDataProvider.AddIterations(iterationsDto));
            }

            if (runTeams)
            {
                var teams = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Teams", () => TeamsService.GetTeamsAsync());
                selectedTeams = TeamsService.GetSelectedTeams(SettingsService.CurrentInputs.Teams, teams);
                selected_teams_planned = selectedTeams.Where(s => s.ReportIds.Contains((int)Reports.PlannedDone));
                selected_teams_employee_reporting = selectedTeams.Where(s => s.ReportIds.Contains((int)Reports.EmployeeReporting));
                selected_teams_tags = selectedTeams.Where(s => s.ReportIds.Contains((int)Reports.Tags));
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Teams", () => SqlDataProvider.AddTeams(selectedTeams));
            }

            if (runCapacities)
            {
                var capacitiesDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Capacities", () => CapacitiesService.GetCapacitiesAsync(selected_teams_employee_reporting, iterationsDto));
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Capacities", () => SqlDataProvider.AddCapacities(capacitiesDto));
            }

            if (runWorkItems)
            {
                workItemResult = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get WorkItems", () => WorkItemService.GetWorkItemsAsync(selected_teams_tags, iterationsDto, SettingsService.CurrentInputs.Tags, runTags));

                if (workItemResult.WorkItemDetailsDtos != null)
                    ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItems", () => SqlDataProvider.AddUpdateWorkItems(workItemResult.WorkItemDetailsDtos));

                if (runTags && workItemResult.WorkItemTags != null)
                    ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItemTags", () => SqlDataProvider.AddWorkItemTags(workItemResult.WorkItemTags));
            }

            if (runEmployees && workItemResult.WorkItemDetails != null)
            {
                var employeesDto = ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Employees", () => EmployeeService.GetEmployees(workItemResult.WorkItemDetails));
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Employees", () => SqlDataProvider.AddEmployees(employeesDto));
            }

            if (runAreas)
            {
                var areas = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Areas", () => AreaService.GetAreasAsync());
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Areas", () => SqlDataProvider.AddAreas(areas));
            }

            if (runPlannedDone)
            {
                var workItemPlannedData = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get WorkItemPlannedData", () => WorkItemPlannedService.GetWorkItemPlannedData(selected_teams_planned, iterationsDto));
                ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItems Planned/Done", () => SqlDataProvider.AddUpdateWorkItemsPlannedDone(workItemPlannedData));
            }

            Console.WriteLine($"Total Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");
        }

        enum Reports
        {
            PlannedDone = 1,
            EmployeeReporting = 2,
            Tags = 3
        }
    }
}
