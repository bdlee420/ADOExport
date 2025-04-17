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
            string jsonString = await File.ReadAllTextAsync("inputs.json");
            var inputs = JsonConvert.DeserializeObject<Inputs>(jsonString);            

            if (inputs == null)
                return;

            CurrentInputs = inputs;

            try
            {
                string jsonOverrideString = await File.ReadAllTextAsync("inputs.override.json");
                var inputsOverride = JsonConvert.DeserializeObject<Inputs>(jsonOverrideString);

                if (inputsOverride == null)
                {
                    UpdateAreas(CurrentInputs.Teams);
                    return;
                }

                if (inputsOverride.Teams.AnySafe())
                    CurrentInputs.Teams.AddRange(inputsOverride.Teams);

                if (inputsOverride.Iterations.AnySafe())
                    CurrentInputs.Iterations.AddRange(inputsOverride.Iterations);

                if (inputsOverride.NameOverrides.AnySafe())
                    CurrentInputs.NameOverrides.AddRange(inputsOverride.NameOverrides);

                if (inputsOverride.TeamMembers.AnySafe())
                    CurrentInputs.TeamMembers.AddRange(inputsOverride.TeamMembers);

                if (inputsOverride.TeamsOverrides.AnySafe())
                    CurrentInputs.Teams = inputsOverride.TeamsOverrides;

                if (inputsOverride.Tags.AnySafe())
                    CurrentInputs.Tags = inputsOverride.Tags;

                if (inputsOverride.RunSettings != null)
                    CurrentInputs.RunSettings = inputsOverride.RunSettings;

                UpdateAreas(CurrentInputs.Teams);
            }
            catch
            {
                Console.WriteLine("no input override file found.");
            }
        }

        private static void UpdateAreas(List<TeamOverrides> teams)
        {
            foreach (var team in teams)
                team.AreaName = team.AreaName ?? team.TeamName;
        }

        internal async static Task SetCurrentSettingsAsync()
        {
            string jsonString = await File.ReadAllTextAsync("appsettings.json");
            var settings = JsonConvert.DeserializeObject<Settings>(jsonString);

            if (settings == null)
                return;

            try
            {
                string jsonOverrideString = await File.ReadAllTextAsync("appsettings.override.json");
                var settingsOverride = JsonConvert.DeserializeObject<Settings>(jsonOverrideString);

                if (settingsOverride == null)
                {
                    CurrentSettings = settings;
                    return;
                }

                if (!string.IsNullOrEmpty(settingsOverride.PersonalAccessToken))
                    settings.PersonalAccessToken = settingsOverride.PersonalAccessToken;

                if (!string.IsNullOrEmpty(settingsOverride.AuthToken))
                    settings.AuthToken = settingsOverride.AuthToken;

                if (!string.IsNullOrEmpty(settingsOverride.ProjectId))
                    settings.ProjectId = settingsOverride.ProjectId;

                if (!string.IsNullOrEmpty(settingsOverride.ProjectName))
                    settings.ProjectName = settingsOverride.ProjectName;

                if (!string.IsNullOrEmpty(settingsOverride.ProjectNameUrl))
                    settings.ProjectNameUrl = settingsOverride.ProjectNameUrl;

                if (!string.IsNullOrEmpty(settingsOverride.ConnectionString))
                    settings.ConnectionString = settingsOverride.ConnectionString;

                if (!string.IsNullOrEmpty(settingsOverride.RootUrl))
                    settings.RootUrl = settingsOverride.RootUrl;
            }
            catch
            {
                Console.WriteLine("no settings override file found.");
            }
            CurrentSettings = settings;
        }
    }
}
