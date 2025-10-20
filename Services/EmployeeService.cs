using ADOExport.Models;

namespace ADOExport.Services
{
    internal class EmployeeService
    {
        internal static List<TeamMemberDto> GetEmployees(List<WorkItemDetails> workItemDetails)
        {
            if (SettingsService.CurrentInputs is null)
                throw new NullReferenceException("SettingsService.CurrentInputs");

            var employees = workItemDetails
                .Select(w => w.Fields.AssignedTo)
                .Distinct()
                .Where(t => t != null)
                .ToList();

            Console.WriteLine($"Get Employees Count = {SettingsService.CurrentInputs.TeamMembers.Count}");

            if (employees.Count == 0)
                return SettingsService.CurrentInputs.TeamMembers;

            foreach (var employee in employees)
            {
                var overrideName = SettingsService.CurrentInputs.NameOverrides.FirstOrDefault(n => n.ADOName == employee.DisplayName);
                if (overrideName != null)
                    employee.DisplayName = overrideName.NewName;
            }

            var employeesToUpdate = employees.Select(e =>
            {
                return new TeamMemberDto
                {
                    EmployeeAdoId = e.Id,
                    Name = e.DisplayName,
                    TeamName = "",
                    Activity = ""
                };
            });

            foreach (var teamMember in SettingsService.CurrentInputs.TeamMembers)
            {
                var employee = employees.FirstOrDefault(e => e.DisplayName == teamMember.Name);
                if (employee != null)
                    teamMember.EmployeeAdoId = employee.Id;
            }

            return SettingsService.CurrentInputs.TeamMembers;
        }

        internal static List<TeamMemberDto> GetEmployees2(List<WorkItemDetails> workItemDetails)
        {
            if (SettingsService.CurrentInputs is null)
                throw new NullReferenceException("SettingsService.CurrentInputs");

            var employees = workItemDetails
                .Select(w => w.Fields.AssignedTo)
                .Distinct()
                .Where(t => t != null)
                .ToList();

            Console.WriteLine($"Get Employees Count = {SettingsService.CurrentInputs.TeamMembers.Count}");

            if (employees.Count == 0)
                return SettingsService.CurrentInputs.TeamMembers;

            foreach (var employee in employees)
            {
                var overrideName = SettingsService.CurrentInputs.NameOverrides.FirstOrDefault(n => n.ADOName == employee.DisplayName);
                if (overrideName != null)
                    employee.DisplayName = overrideName.NewName;
            }

            var employeesToUpdate = employees.Select(e =>
            {
                return new TeamMemberDto
                {
                    EmployeeAdoId = e.Id,
                    Name = e.DisplayName,
                    TeamName = "",
                    Activity = ""
                };
            }).ToList();

            return employeesToUpdate;
        }
    }
}
