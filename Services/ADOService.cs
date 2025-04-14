using System.Net.Http.Headers;
using System.Text;
using ADOExport.Models;
using Newtonsoft.Json;

namespace ADOExport.Services
{
    public class ADOService
    {
        public static async Task<List<WorkItem>> GetWorkItemIds(List<IterationDto> iterations)
        {
            try
            {
                var workitems = new List<WorkItem>();
                foreach (var iteration in iterations)
                {
                    string query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND ( [System.State] IN ('Done', 'Closed') AND [System.IterationPath] = '{SettingsService.CurrentSettings.ProjectName}\\\\Current\\\\Feature Release\\\\{iteration.Name}' AND ";

                    var conditions = SettingsService.CurrentInputs.Teams
                        .Select(team => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                    query += $" ( {string.Join(" OR ", conditions)} ) ";
                    query += " AND ( [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Defect' OR [System.WorkItemType] = 'Deployment' )) ORDER BY [System.IterationPath], [System.AssignedTo]";
                    var queryRequest = new QueryRequest
                    {
                        Query = query
                    };

                    var queryResponse = await PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                    workitems.AddRange(queryResponse.WorkItems);
                }
                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<List<WorkItemDetails>> GetWorkItemDetails(List<int> workItemIds)
        {
            try
            {
                var workItemDetails = new List<WorkItemDetails>();
                for (int i = 0; i < workItemIds.Count; i += 200)
                {
                    // Get the current batch of 200 integers
                    var batch = workItemIds.GetRange(i, Math.Min(200, workItemIds.Count - i));

                    var workItemIdsString = string.Join(",", batch);

                    var url = $"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/workitems?ids={workItemIdsString}&fields=Id,System.AssignedTo,Microsoft.VSTS.Scheduling.OriginalEstimate,System.WorkItemType,System.IterationPath,System.IterationId,System.AreaPath,System.AreaId&api-version=7.1";

                    var queryResponse = await GetAsync<WorkItemReponse>(url, SettingsService.CurrentSettings.PersonalAccessToken);

                    workItemDetails.AddRange(queryResponse.Value);
                }

                return workItemDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<List<CapacityResult>> GetCapacities(List<IterationDto> iterations, List<Team> teams)
        {
            try
            {
                var capacityResults = new List<CapacityResult>();
                foreach (var iteration in iterations)
                {
                    foreach (var team in teams)
                    {
                        try
                        {
                            var capacityResponse = await GetAsync<CapacityResponse>($"{SettingsService.CurrentSettings.ProjectId}/{team.Id}/_apis/work/teamsettings/iterations/{iteration.Identifier}/capacities", SettingsService.CurrentSettings.PersonalAccessToken);
                            var capacityResult = new CapacityResult
                            {
                                Iteration = iteration,
                                Team = team,
                                TeamMembers = capacityResponse.TeamMembers
                            };
                            capacityResults.Add(capacityResult);
                        }
                        catch (HttpRequestException ex)
                        {
                            if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                            {
                                throw;
                            }
                        }
                    }
                }
                return capacityResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<List<Team>> GetTeams()
        {
            try
            {
                var teamResponse = await GetAsync<TeamResponse>($"_apis/projects/{SettingsService.CurrentSettings.ProjectName}/teams?api-version=6.0", SettingsService.CurrentSettings.PersonalAccessToken);
                return teamResponse.Value;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<List<Area>> GetAreas()
        {
            try
            {
                var areaResponse = await GetAsync<AreaResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/classificationNodes?$depth=1&api-version=6.0\r\n", SettingsService.CurrentSettings.PersonalAccessToken);
                return areaResponse.Value
                    .First(v => v.Name == SettingsService.CurrentSettings.ProjectName)
                    .Children;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<List<Iteration>> GetIterations()
        {
            try
            {
                var iterationRequest = new IterationRequest
                {
                    ContributionIds = new List<string>
                     {
                         "ms.vss-work-web.new-team-wit-settings-data-provider"
                     },
                    DataProviderContext = new DataProviderContext
                    {
                        Properties = new DataProviderContextProperties
                        {
                            SourcePage = new SourcePage
                            {
                                RouteId = "ms.vss-work-web.new-sprints-content-route",
                                RouteValues = new RouteValues
                                {
                                    Project = SettingsService.CurrentSettings.ProjectName,
                                    Pivot = "capacity",
                                    TeamName = SettingsService.CurrentInputs.Teams.First().TeamName,
                                    Viewname = "content",
                                }
                            }
                        }
                    }
                };
                var url = "_apis/Contribution/HierarchyQuery?api-version=5.0-preview.1";
                var iterationResponse = await PostAsync<IterationRequest, IterationResponse>(url, iterationRequest, SettingsService.CurrentSettings.AuthToken);
                var iterations = iterationResponse.DataProviders.DataProviderMeta.PreviousIterations;
                iterations.Add(iterationResponse.DataProviders.DataProviderMeta.CurrentIteration);
                return iterations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private static void SetClientHeaders(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", token))));
        }

        private static async Task<Tres> PostAsync<Treq, Tres>(string url, Treq request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetClientHeaders(client, token);

                var iterationRequestString = JsonConvert.SerializeObject(request);
                var content = new StringContent(iterationRequestString, Encoding.UTF8, "application/json");
                url = $"https://dev.azure.com/{SettingsService.CurrentSettings.RootUrl}/{url}";
                using (HttpResponseMessage response = client.PostAsync(url, content).Result)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<Tres>(responseBody);
                    }
                    else if(string.IsNullOrEmpty( responseBody))
                    {
                        Console.WriteLine($"Status Code: {response.StatusCode}");
                        throw new Exception("Failed");
                    }
                    {
                        var error = JsonConvert.DeserializeObject<ErrorReponse>(responseBody);
                        Console.WriteLine($"Status Code: {response.StatusCode}; Message: {error.Message}");
                        throw new Exception(error.Message);
                    }
                }
            }
        }

        private static async Task<Tres> GetAsync<Tres>(string url, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetClientHeaders(client, token);
                url = $"https://dev.azure.com/{SettingsService.CurrentSettings.RootUrl}/{url}";
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Tres>(responseBody);
                }
            }
        }
    }
}
