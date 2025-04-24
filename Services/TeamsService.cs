using ADOExport.Models;

namespace ADOExport.Services
{
    internal class TeamsService
    {
        internal async static Task<List<Team>> GetTeamsAsync()
        {
            var teams = await ADOService.GetTeams();

            Console.WriteLine($"Get Teams Count = {teams.Count}");

            return teams;
        }

        internal static List<Team> GetSelectedTeams(List<TeamOverrides> requestedTeams, List<Team> allTeams)
        {
            foreach (var team in allTeams)
                team.AreaName = team.Name;

            if (requestedTeams.Count == 0)
                return allTeams;

            var selectedTeams = new List<Team>();
            foreach (var team in allTeams)
            {
                var selectedTeam = requestedTeams.FirstOrDefault(rt => rt.TeamName == team.Name);
                if (selectedTeam != null)
                {
                    team.AreaName = selectedTeam.AreaName;
                    team.ReportIds = selectedTeam.ReportIds;
                    selectedTeams.Add(team);
                }
            }

            Console.WriteLine($"Get Selected Teams Count = {selectedTeams.Count}");

            return selectedTeams;
        }
    }
}
