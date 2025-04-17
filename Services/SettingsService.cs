using ADOExport.Common;
using ADOExport.Models;
using Newtonsoft.Json;

namespace ADOExport.Services
{
    internal static class SettingsService
    {
        internal static Settings? CurrentSettings = null;
        internal static Inputs? CurrentInputs = null;

        internal async static Task SetInputsAsync()
        {
            string jsonOverrideString = await File.ReadAllTextAsync("inputs.override.json");
            var inputsOverride = JsonConvert.DeserializeObject<Inputs>(jsonOverrideString);

            if (inputsOverride == null)
                return;

            CurrentInputs = inputsOverride;

            if (inputsOverride.TeamsOverrides.AnySafe())
                CurrentInputs.Teams = inputsOverride.TeamsOverrides;

            UpdateAreas(CurrentInputs.Teams);            
        }

        private static void UpdateAreas(List<TeamOverrides> teams)
        {
            foreach (var team in teams)
                team.AreaName = team.AreaName ?? team.TeamName;
        }

        internal async static Task SetCurrentSettingsAsync()
        {
            string jsonOverrideString = await File.ReadAllTextAsync("appsettings.override.json");
            var settingsOverride = JsonConvert.DeserializeObject<Settings>(jsonOverrideString);
            
            if (settingsOverride == null)
            {
                return;
            }

            CurrentSettings = settingsOverride;
        }
    }
}
