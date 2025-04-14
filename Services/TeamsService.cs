using ADOExport.Models;

namespace ADOExport.Services
{
    internal class TeamsService
    {
        internal async static Task<List<Team>> GetTeamsAsync(List<TeamOverrides> requestedTeams)
        {
            var teams = await ADOService.GetTeams();

            if (requestedTeams.Count == 0)
                return teams;

            var selectedTeams = teams
                .Where(t => requestedTeams.Exists(rt => rt.TeamName == t.Name))
                .ToList();

            return selectedTeams;
        }
    }
}
