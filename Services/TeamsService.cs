using ADOExport.Models;

namespace ADOExport.Services
{
    internal class TeamsService
    {
        internal async static Task<List<Team>> GetTeamsAsync(List<string> requestedTeams)
        {
            var teams = await ADOService.GetTeams();

            var selectedTeams = teams
                .Where(t => requestedTeams.Contains(t.Name))
                .ToList();

            return selectedTeams;
        }
    }
}
