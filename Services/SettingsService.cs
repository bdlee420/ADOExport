using ADOExport.Models;
using Newtonsoft.Json;

namespace ADOExport.Services
{
    internal static class SettingsService
    {
        internal static Settings CurrentSettings = new();
        internal static Inputs CurrentInputs = new();

        internal async static Task SetInputsAsync()
        {
            string jsonString = await File.ReadAllTextAsync("inputs.json");
            var inputs = JsonConvert.DeserializeObject<Inputs>(jsonString);

            if (inputs == null)
                return;

            try
            {
                string jsonOverrideString = await File.ReadAllTextAsync("inputs.override.json");
                var inputsOverride = JsonConvert.DeserializeObject<Inputs>(jsonOverrideString);

                if (inputsOverride == null)
                    return;

                inputs.Teams.AddRange(inputsOverride.Teams);
                inputs.Iterations.AddRange(inputsOverride.Iterations);
                inputs.NameOverrides.AddRange(inputsOverride.NameOverrides);
                inputs.TeamMembers.AddRange(inputsOverride.TeamMembers);
            }
            catch
            {
                Console.WriteLine("no input override file found.");
            }

            CurrentInputs = inputs;
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
