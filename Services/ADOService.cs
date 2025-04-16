using ADOExport.Common;
using ADOExport.Models;

namespace ADOExport.Services
{
    internal class ADOService
    {
        internal static async Task<List<WorkItem>> GetWorkItemIdsAsOf_Start(DateTime asOf, IterationDto iteration, IEnumerable<Team> teams)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItem>();

                string query = $@"  SELECT [System.Id] 
                                    FROM workitems 
                                    WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.State] NOT IN ('Done', 'Closed', 'Removed') 
                                    AND ( [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Defect')
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}'";

                var conditionsAreas = teams.Select(t => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{t.AreaName}'");
                query += $" AND ( {string.Join(" OR ", conditionsAreas)} ) ";

                query += $" ASOF '{asOf}' ";

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetWorkItemIdsAsOf_End(DateTime asOf, List<int> ids, IterationDto iteration)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItem>();

                string query = $@"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.State] IN ('Done', 'Closed', 'Removed') 
                                    AND [System.Id] IN ({string.Join(",", ids)}) 
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}'
                                    ASOF '{asOf}'";               

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetWorkItemIdsAsOf_End_NoFilter(DateTime asOf, List<int> ids, IterationDto iteration)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItem>();

                string query = $@"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.Id] IN ({string.Join(",", ids)}) 
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}'
                                    ASOF '{asOf}'";

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetWorkItemIds(IEnumerable<Team> teams, List<IterationDto> iterations)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItem>();
                foreach (var iteration in iterations)
                {
                    string query = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND ( [System.State] IN ('Done', 'Closed') AND [System.IterationPath] = '{SettingsService.CurrentSettings.ProjectName}\\\\Current\\\\Feature Release\\\\{iteration.Name}' AND ";

                    var conditions = teams.Select(team => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                    query += $" ( {string.Join(" OR ", conditions)} ) ";
                    query += " AND ( [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Defect' OR [System.WorkItemType] = 'Deployment' )) ORDER BY [System.IterationPath], [System.AssignedTo]";
                    var queryRequest = new QueryRequest
                    {
                        Query = query
                    };

                    var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                    workitems.AddRange(queryResponse.WorkItems);
                }
                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItemChildren>> GetWorkItemIdsByTag(IEnumerable<Team> teams, List<IterationDto> iterations, string tag)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItemChildren>();
                foreach (var iteration in iterations)
                {
                    string query = $"SELECT [System.Id] FROM workitemLinks WHERE ([Source].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' ";
                    query += $" AND [Source].[System.Tags] CONTAINS '{tag}' AND ( [Source].[System.WorkItemType] = 'User Story' OR [Source].[System.WorkItemType] = 'Bug' OR [Source].[System.WorkItemType] = 'Deployment' ))";
                    query += $"AND ([Target].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND [Target].[System.State] IN ('Done', 'Closed') AND [Target].[System.WorkItemType] = 'Task' ";
                    var conditions = teams.Select(team => $"[Target].[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                    query += $" AND ( {string.Join(" OR ", conditions)} ) ";
                    query += $" AND [Target].[System.IterationPath] = '{SettingsService.CurrentSettings.ProjectName}\\\\Current\\\\Feature Release\\\\{iteration.Name}' ";
                    query += $") ORDER BY [System.Id] MODE (MustContain)";


                    var queryRequest = new QueryRequest
                    {
                        Query = query
                    };

                    var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryChildrenResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                    workitems.AddRange(queryResponse.WorkItemRelations);
                }
                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItemDetails>> GetWorkItemDetails(List<int> workItemIds, DateTime? asOf = null)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                var workItemDetails = new List<WorkItemDetails>();
                for (int i = 0; i < workItemIds.Count; i += 200)
                {
                    // Get the current batch of 200 integers
                    var batch = workItemIds.GetRange(i, Math.Min(200, workItemIds.Count - i));

                    var workItemIdsString = string.Join(",", batch);

                    var asOfFilter = asOf.HasValue ? $"asOf={asOf.Value}&" : string.Empty;

                    var url = $"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/workitems?ids={workItemIdsString}&fields=Id,System.AssignedTo,Microsoft.VSTS.Scheduling.OriginalEstimate,System.WorkItemType,System.IterationPath,System.IterationId,System.AreaPath,System.AreaId&{asOfFilter}api-version=7.1";

                    var queryResponse = await WebClientHelper.GetAsync<WorkItemReponse>(url, SettingsService.CurrentSettings.PersonalAccessToken);

                    workItemDetails.AddRange(queryResponse.Value);
                }

                return workItemDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<CapacityResult>> GetCapacities(List<IterationDto> iterations, IEnumerable<Team> teams)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                var capacityResults = new List<CapacityResult>();
                foreach (var iteration in iterations)
                {
                    foreach (var team in teams)
                    {
                        try
                        {
                            var capacityResponse = await WebClientHelper.GetAsync<CapacityResponse>($"{SettingsService.CurrentSettings.ProjectId}/{team.Id}/_apis/work/teamsettings/iterations/{iteration.Identifier}/capacities", SettingsService.CurrentSettings.PersonalAccessToken);
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
                throw;
            }
        }

        internal static async Task<List<Team>> GetTeams()
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                var teamResponse = await WebClientHelper.GetAsync<TeamResponse>($"_apis/projects/{SettingsService.CurrentSettings.ProjectName}/teams?api-version=6.0", SettingsService.CurrentSettings.PersonalAccessToken);
                return teamResponse.Value;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<Area>> GetAreas()
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                var areaResponse = await WebClientHelper.GetAsync<AreaResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/classificationNodes?$depth=1&api-version=6.0\r\n", SettingsService.CurrentSettings.PersonalAccessToken);
                return areaResponse.Value
                    .First(v => v.Name == SettingsService.CurrentSettings.ProjectName)
                    .Children;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<Iteration>> GetIterations()
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var iterationRequest = new IterationRequest
                {
                    ContributionIds =
                     [
                         "ms.vss-work-web.new-team-wit-settings-data-provider"
                     ],
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
                var iterationResponse = await WebClientHelper.PostAsync<IterationRequest, IterationResponse>(url, iterationRequest, SettingsService.CurrentSettings.AuthToken);
                var iterations = iterationResponse.DataProviders.DataProviderMeta.PreviousIterations;
                iterations.Add(iterationResponse.DataProviders.DataProviderMeta.CurrentIteration);
                return iterations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
