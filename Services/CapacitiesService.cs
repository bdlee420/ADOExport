using ADOExport.Common;
using ADOExport.Models;

namespace ADOExport.Services
{
    internal class CapacitiesService
    {
        internal async static Task<List<CapacityDto>> GetCapacitiesAsync(List<IterationDto> iterations, List<Team> teams)
        {
            var capacities = await ADOService.GetCapacities(iterations, teams);
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
                    foreach (var _ in teamMember.Activities)
                    {
                        capacitiesDto.Add(new CapacityDto
                        {
                            EmployeeAdoId = teamMember.TeamMember.Id,
                            IterationAdoId = capacity.Iteration.Id,
                            IterationAdoIdentifier = capacity.Iteration.Identifier,
                            TeamAdoId = capacity.Team.Id,
                            Days = daysWorked
                        });
                    }
                }
            }

            var uniqueCapacitiesDto = capacitiesDto
               .GroupBy(e => new { e.EmployeeAdoId, e.IterationAdoIdentifier, e.TeamAdoId }) // Group by both properties
               .Select(g => g.First()) // Select the first entry from each group
               .ToList();

            return uniqueCapacitiesDto;
        }
    }
}
