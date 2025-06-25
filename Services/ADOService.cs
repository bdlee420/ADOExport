using ADOExport.Common;
using ADOExport.Models;

namespace ADOExport.Services
{
    internal class ADOService
    {
        private static string GetUTCDateString(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 4, 0, 0, DateTimeKind.Utc).ToString("o");
        }

        internal static async Task<List<WorkItem>> GetNotDoneWorkItemIdsAsOf_Start(DateTime asOf, IterationDto iteration, IEnumerable<Team> teams)
        {
            try
            {
                var asOfString = GetUTCDateString(asOf);

                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitems = new List<WorkItem>();

                string query = $@"  SELECT [System.Id] 
                                    FROM workitems 
                                    WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.State] NOT IN ('Done', 'Closed', 'Removed') 
                                    AND [System.WorkItemType] IN ('Defect', 'Task')  
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}'";

                var conditionsAreas = teams.Select(t => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{t.AreaName}'");
                query += $" AND ( {string.Join(" OR ", conditionsAreas)} ) ";

                query += $" ASOF '{asOfString}' ";

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=10000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetDoneWorkItemIdsAsOf_End(DateTime asOf, IterationDto iteration, IEnumerable<Team> teams)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var asOfString = GetUTCDateString(asOf);

                var workitems = new List<WorkItem>();
                //AND [System.Id] IN ({string.Join(",", ids)}) 
                string query = $@"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.State] IN ('Done', 'Closed', 'Removed')                                     
                                    AND [System.WorkItemType] IN ('Defect', 'Task')  
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}' ";

                var conditionsAreas = teams.Select(t => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{t.AreaName}'");
                query += $" AND ( {string.Join(" OR ", conditionsAreas)} ) ";

                query += $" ASOF '{asOfString}' ";

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=10000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetDoneWorkItemIdsAsOf_Start(DateTime asOf, IterationDto iteration, IEnumerable<int> ids)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var asOfString = GetUTCDateString(asOf);

                var workitems = new List<WorkItem>();
                //AND [System.Id] IN ({string.Join(",", ids)}) 
                string query = $@"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.State] IN ('Done', 'Closed', 'Removed')             
                                    AND [System.WorkItemType] IN ('Defect', 'Task')  
                                    AND [System.Id] IN ({string.Join(",", ids)})  
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}' ";

                query += $" ASOF '{asOfString}' ";

                var queryRequest = new QueryRequest
                {
                    Query = query
                };

                var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=10000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                workitems.AddRange(queryResponse.WorkItems);

                return workitems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItem>> GetAllWorkItemIdsAsOf_End(DateTime asOf, IterationDto iteration, IEnumerable<Team> teams)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var asOfString = GetUTCDateString(asOf);

                var workitems = new List<WorkItem>();
                //AND [System.Id] IN ({string.Join(",", ids)}) 
                string query = $@"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' 
                                    AND [System.WorkItemType] IN ('Defect', 'Task')  
                                    AND [System.IterationPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\Current\Feature Release\{iteration.Name}' ";

                var conditionsAreas = teams.Select(t => $"[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{t.AreaName}'");
                query += $" AND ( {string.Join(" OR ", conditionsAreas)} ) ";

                query += $" ASOF '{asOfString}' ";

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
                    query += " AND ( [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Defect')) ORDER BY [System.IterationPath], [System.AssignedTo]";
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

        internal static async Task<Dictionary<string, List<WorkItemChildren>>> GetWorkItemIdsByTag(IEnumerable<Team> teams, List<IterationDto> iterations, List<string> tags)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitemsByTag = new Dictionary<string, List<WorkItemChildren>>();
                foreach (var tag in tags)
                {
                    var workitems = new List<WorkItemChildren>();
                    foreach (var iteration in iterations)
                    {
                        string query = $"SELECT [System.Id] FROM workitemLinks WHERE ([Source].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' ";

                        query += $" AND [Source].[System.Tags] CONTAINS '{tag}' ";
                        query += $" AND ( [Source].[System.WorkItemType] = 'Epic' OR [Source].[System.WorkItemType] = 'Feature' OR [Source].[System.WorkItemType] = 'User Story' OR [Source].[System.WorkItemType] = 'Bug' OR [Source].[System.WorkItemType] = 'Deployment' ))";
                        query += $"AND ([Target].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND [Target].[System.State] IN ('Done', 'Closed') AND [Target].[System.WorkItemType] = 'Task' ";
                        var conditions = teams.Select(team => $"[Target].[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                        query += $" AND ( {string.Join(" OR ", conditions)} ) ";
                        query += $" AND [Target].[System.IterationPath] = '{SettingsService.CurrentSettings.ProjectName}\\\\Current\\\\Feature Release\\\\{iteration.Name}' ";
                        query += $")";
                        query += " AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') ";
                        query += " ORDER BY [System.Id] MODE (Recursive)";

                        var queryRequest = new QueryRequest
                        {
                            Query = query
                        };

                        var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryChildrenResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                        workitems.AddRange(queryResponse.WorkItemRelations);
                    }
                    workitemsByTag[tag] = workitems;
                }

                return workitemsByTag;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<Dictionary<string, List<WorkItemChildren>>> GetWorkItemIdsByTag(IEnumerable<Team> teams, List<string> tags)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitemsByTag = new Dictionary<string, List<WorkItemChildren>>();
                foreach (var tag in tags)
                {
                    var workitems = new List<WorkItemChildren>();
                    string query = $"SELECT [System.Id] FROM workitemLinks WHERE ([Source].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND (";

                    bool isFirst = true;
                    foreach (var tag_part in tag.Split("|"))
                    {
                        if (!isFirst)
                            query += $" AND ";
                        isFirst = false;
                        query += $" [Source].[System.Tags] CONTAINS '{tag_part}' ";
                    }

                    query += $") AND ( [Source].[System.WorkItemType] = 'Epic' OR [Source].[System.WorkItemType] = 'Feature' OR [Source].[System.WorkItemType] = 'User Story' OR [Source].[System.WorkItemType] = 'Bug' OR [Source].[System.WorkItemType] = 'Deployment' ))";
                    query += $"AND ([Target].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND [Target].[System.State] NOT IN ('Removed', 'Done', 'Closed') AND [Target].[System.WorkItemType] = 'Task' ";
                    var conditions = teams.Select(team => $"[Target].[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                    query += $" AND ( {string.Join(" OR ", conditions)} ) ";
                    query += $")";
                    query += " AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') ";
                    query += " ORDER BY [System.Id] MODE (Recursive)";

                    var queryRequest = new QueryRequest
                    {
                        Query = query
                    };

                    var queryResponse = await WebClientHelper.PostAsync<QueryRequest, QueryChildrenResponse>($"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/wiql?$top=1000&api-version=7.1", queryRequest, SettingsService.CurrentSettings.PersonalAccessToken);
                    workitems.AddRange(queryResponse.WorkItemRelations);
                    workitemsByTag[tag] = workitems;
                }

                return workitemsByTag;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        internal static async Task<List<WorkItemChildren>> GetWorkItemIds(IEnumerable<Team> teams, List<IterationDto> iterations, string parentType)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var workitemsByTag = new Dictionary<string, List<WorkItemChildren>>();
                var workitems = new List<WorkItemChildren>();
                foreach (var iteration in iterations)
                {
                    string query = $"SELECT [System.Id] FROM workitemLinks WHERE ([Source].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' ";

                    query += $" AND ( [Source].[System.WorkItemType] = '{parentType}'))";
                    query += $" AND ([Target].[System.TeamProject] = '{SettingsService.CurrentSettings.ProjectName}' AND [Target].[System.State] IN ('Done', 'Closed') ";
                    query += $" AND ( [Target].[System.WorkItemType] = 'Task' OR [Target].[System.WorkItemType] = 'Defect') ";
                    var conditions = teams.Select(team => $"[Target].[System.AreaPath] UNDER '{SettingsService.CurrentSettings.ProjectName}\\\\{team.AreaName}'");
                    query += $" AND ( {string.Join(" OR ", conditions)} ) ";
                    query += $" AND [Target].[System.IterationPath] = '{SettingsService.CurrentSettings.ProjectName}\\\\Current\\\\Feature Release\\\\{iteration.Name}' ";
                    query += $")";
                    query += " ORDER BY [System.Id] MODE (MustContain)";

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

                    var asOfFilter = asOf.HasValue ? $"asOf={GetUTCDateString(asOf.Value)}&" : string.Empty;

                    var url = $"{SettingsService.CurrentSettings.ProjectName}/_apis/wit/workitems?ids={workItemIdsString}&fields=Id,System.AssignedTo,Microsoft.VSTS.Scheduling.OriginalEstimate,System.WorkItemType,System.IterationPath,Microsoft.VSTS.Scheduling.RemainingWork,Microsoft.VSTS.Common.Activity,System.IterationId,System.AreaPath,System.State,System.AreaId&{asOfFilter}api-version=7.1";

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
                var error = ex.ToString();
                if (!error.Contains("Unauthorized"))
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    Console.WriteLine("Invalid authToken: Unable to load data from ADO. Will use existing iterations from database instead.");
                }
                throw;
            }
        }

        internal static async Task<List<CapacityResult>> GetCapacities2(List<IterationDto> iterations, IEnumerable<Team> teams)
        {
            try
            {
                if (SettingsService.CurrentSettings is null)
                    throw new NullReferenceException("SettingsService.CurrentSettings");

                if (SettingsService.CurrentInputs is null)
                    throw new NullReferenceException("SettingsService.CurrentInputs");

                var capacityResults = new List<CapacityResult>();
                foreach (var iteration in iterations)
                {
                    foreach (var team in teams)
                    {
                        try
                        {
                            var capacityRequest = new IterationRequest
                            {
                                ContributionIds =
                             [
                                 "ms.vss-work-web.sprints-hub-capacity-data-provider"
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
                                                TeamName = team.Name,
                                                Viewname = "content",
                                                Iteration = $"{SettingsService.CurrentSettings.ProjectName}/Current/Feature Release/{iteration.Name}"
                                            }
                                        }
                                    }
                                }
                            };
                            var url = "_apis/Contribution/HierarchyQuery?api-version=5.0-preview.1";
                            var capacityResponse = await WebClientHelper.PostAsync<IterationRequest, CapacityResponse>(url, capacityRequest, SettingsService.CurrentSettings.AuthToken);

                            var capacityResult = new CapacityResult
                            {
                                Team = team,
                                Iteration = iteration,
                                TeamMembers = []
                            };

                            capacityResult.TeamMembers = capacityResponse.DataProviders.DataProviderMeta.UserCapacities.Select(c => new TeamMemberCapacity
                            {
                                TeamMember = c.TeamMember,
                                Activities = c.Activities,
                                DaysOff = c.DaysOff
                            }).ToList();

                            capacityResults.Add(capacityResult);
                        }
                        catch
                        {
                            continue;
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
    }
}
