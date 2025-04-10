using ADOExport.Data;
using ADOExport.Services;

namespace ADOExport
{
    internal class Program
    {
        static async Task Main()
        {
            await SettingsService.SetCurrentSettingsAsync();
            await SettingsService.SetInputsAsync();
            if (SettingsService.CurrentInputs == null || SettingsService.CurrentInputs.Teams.Count == 0 || SettingsService.CurrentInputs.Iterations.Count == 0)
            {
                Console.WriteLine("No inputs!");
                return;
            }

            var selectedTeams = await TeamsService.GetTeamsAsync(SettingsService.CurrentInputs.Teams);
            var iterationsDto = await IterationsService.GetIterationsAsync(SettingsService.CurrentInputs.Iterations);
            var capacitiesDto = await CapacitiesService.GetCapacitiesAsync(iterationsDto, selectedTeams);
            var workItemResult = await WorkItemService.GetWorkItemsAsync(iterationsDto);
            var employeesDto = EmployeeService.GetEmployees(workItemResult.WorkItemDetails);

            SqlDataProvider.ClearData();
            SqlDataProvider.AddAllData(workItemResult.WorkItemDetailsDtos);
            SqlDataProvider.AddCapacities(capacitiesDto);
            SqlDataProvider.AddEmployees(employeesDto);
            SqlDataProvider.AddIterations(iterationsDto);
        }
    }
}
