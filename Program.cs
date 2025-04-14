using System.Diagnostics;
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

            Console.WriteLine("Get Teams Start");
            var elapsedStart = stopwatch.ElapsedMilliseconds;
            var selectedTeams = await TeamsService.GetTeamsAsync(SettingsService.CurrentInputs.Teams);
            Console.WriteLine($"Get Teams Count: {selectedTeams.Count}");
            Console.WriteLine($"Get Teams Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Get Iterations Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            var iterationsDto = await IterationsService.GetIterationsAsync(SettingsService.CurrentInputs.Iterations);
            Console.WriteLine($"Get Iterations Count: {iterationsDto.Count}");
            Console.WriteLine($"Get Iterations Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Get Capacities Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            var capacitiesDto = await CapacitiesService.GetCapacitiesAsync(iterationsDto, selectedTeams);
            Console.WriteLine($"Get Capacities Count: {capacitiesDto.Count}");
            Console.WriteLine($"Get Capacities Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Get WorkItems Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            var workItemResult = await WorkItemService.GetWorkItemsAsync(iterationsDto);
            Console.WriteLine($"Get WorkItems Count: {workItemResult.WorkItemDetails.Count}");
            Console.WriteLine($"Get WorkItems: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Get Employees Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            var employeesDto = EmployeeService.GetEmployees(workItemResult.WorkItemDetails);
            Console.WriteLine($"Get Employees Count: {employeesDto.Count}");
            Console.WriteLine($"Get Employees Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Clear WorkItems Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.ClearWorkItems(workItemResult.WorkItemDetailsDtos);
            Console.WriteLine($"Clear WorkItems Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Clear Iterations Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.ClearIterations(iterationsDto);
            Console.WriteLine($"Clear Iterations Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Clear Capacities Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.ClearDevCapacity(iterationsDto);
            Console.WriteLine($"Clear Capacities Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Add WorkItems Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.AddWorkItems(workItemResult.WorkItemDetailsDtos);
            Console.WriteLine($"Add WorkItems Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Add Capacities Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.AddCapacities(capacitiesDto);
            Console.WriteLine($"Add Capacities Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Add Employees Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.AddEmployees(employeesDto);
            Console.WriteLine($"Add Employees Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            Console.WriteLine("Add Iterations Start");
            elapsedStart = stopwatch.ElapsedMilliseconds;
            SqlDataProvider.AddIterations(iterationsDto);
            Console.WriteLine($"Add Iterations Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
        }
    }
}
