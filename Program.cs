using System.Diagnostics;
using ADOExport.Common;
using ADOExport.Data;
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
            await SettingsService.SetInputsAsync();
            if (SettingsService.CurrentInputs == null || SettingsService.CurrentInputs.Teams.Count == 0)
            {
                Console.WriteLine("No inputs!");
                return;
            }

            var selectedTeams = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Teams", () => TeamsService.GetTeamsAsync(SettingsService.CurrentInputs.Teams));
            var iterationsDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Iterations", () => IterationsService.GetIterationsAsync(SettingsService.CurrentInputs.Iterations));
            var capacitiesDto = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Capacities", () => CapacitiesService.GetCapacitiesAsync(iterationsDto, selectedTeams));
            var workItemResult = await ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get WorkItems", () => WorkItemService.GetWorkItemsAsync(iterationsDto));
            var employeesDto = ExecuteHelper.ExecuteAndLogAction(stopwatch, "Get Employees", () => EmployeeService.GetEmployees(workItemResult.WorkItemDetails));

            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Clear WorkItems", () => SqlDataProvider.ClearWorkItems(workItemResult.WorkItemDetailsDtos));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Clear Iterations", () => SqlDataProvider.ClearIterations(iterationsDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Clear Capacities", () => SqlDataProvider.ClearDevCapacity(iterationsDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add WorkItems", () => SqlDataProvider.AddWorkItems(workItemResult.WorkItemDetailsDtos));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Capacities", () => SqlDataProvider.AddCapacities(capacitiesDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Employees", () => SqlDataProvider.AddEmployees(employeesDto));
            ExecuteHelper.ExecuteAndLogAction(stopwatch, "Add Iterations", () => SqlDataProvider.AddIterations(iterationsDto));
        }           
    }
}
