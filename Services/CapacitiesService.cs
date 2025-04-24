using ADOExport.Common;
using ADOExport.Models;

namespace ADOExport.Services
{
    internal class CapacitiesService
    {
        internal async static Task<List<CapacityDto>> GetCapacitiesAsync(IEnumerable<Team> teams, List<IterationDto> iterations)
        {
            var capacities = await ADOService.GetCapacities2(iterations, teams);
            var capacitiesDto = new List<CapacityDto>();
            foreach (var capacity in capacities)
            {
                var daysInSprint = LogicHelper.CountBusinessDays(capacity.Iteration.StartDate, capacity.Iteration.EndDate);
                foreach (var teamMember in capacity.TeamMembers)
                {
                    int totalDaysOff = 0;
                    foreach (var daysOff in teamMember.DaysOff)
                    {
                        totalDaysOff += LogicHelper.CountBusinessDays(daysOff.Start, daysOff.End);
                    }
                    var daysWorked = daysInSprint - totalDaysOff;
                    var hasDevelopment = teamMember.Activities.Exists(a => a.Name == "Development" || a.Name == "Back End" || a.Name == "Front End" || a.Name == "BAU Support");

                    if (teamMember.Activities.Exists(a => a.CapacityPerDay > 0))
                    {
                        capacitiesDto.Add(new CapacityDto
                        {
                            EmployeeAdoId = teamMember.TeamMember.Id,
                            IterationAdoId = capacity.Iteration.Id,
                            IterationAdoIdentifier = capacity.Iteration.Identifier,
                            TeamAdoId = capacity.Team.Id,
                            Days = daysWorked,
                            IsDev = hasDevelopment
                        });
                    }
                }
            }

            var uniqueCapacitiesDto = capacitiesDto
               .GroupBy(e => new { e.EmployeeAdoId, e.IterationAdoIdentifier, e.TeamAdoId }) // Group by both properties
               .Select(g => g.First()) // Select the first entry from each group
               .ToList();

            Console.WriteLine($"Get Capacities Count = {uniqueCapacitiesDto.Count}");

            return uniqueCapacitiesDto;
        }
    }
}
