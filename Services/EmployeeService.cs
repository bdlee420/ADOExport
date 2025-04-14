using ADOExport.Models;

namespace ADOExport.Services
{
    internal class EmployeeService
    {
        internal static List<TeamMemberDto> GetEmployees(List<WorkItemDetails> workItemDetails)
        {
            var employees = workItemDetails
                .Select(w => w.Fields.AssignedTo)
                .Distinct()
                .Where(t => t != null)
                .ToList();

            if (employees.Count == 0)
                return SettingsService.CurrentInputs.TeamMembers;

            foreach (var employee in employees)
            {
                var overrideName = SettingsService.CurrentInputs.NameOverrides.FirstOrDefault(n => n.ADOName == employee.DisplayName);
                if (overrideName != null)
                    employee.DisplayName = overrideName.NewName;
            }

            foreach (var teamMember in SettingsService.CurrentInputs.TeamMembers)
            {
                var employee = employees.First(e => e.DisplayName == teamMember.Name);
                teamMember.EmployeeAdoId = employee.Id;
            }

            return SettingsService.CurrentInputs.TeamMembers;
        }
    }
}
