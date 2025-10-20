using System.Data;
using System.Data.SqlClient;
using ADOExport.Models;
using ADOExport.Services;
using Dapper;

namespace ADOExport.Data
{
    internal static class SqlDataProvider
    {
        private static string GetConnectionString()
        {
            var connectionString = (SettingsService.CurrentSettings?.ConnectionString) ?? throw new NullReferenceException("SettingsService.CurrentSettings.ConnectionString");
            return connectionString;
        }

        internal async static Task<List<IterationDto>> GetIterationsAsync()
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();
            var sql = "SELECT Id, Identifier, Name, Path, StartDate, EndDate, YearQuarter FROM Iterations";
            var data = await connection.QueryAsync<IterationDto>(sql);
            return data.ToList();
        }

        internal static void AddCapacities(List<CapacityDto> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #DevCapacity (EmployeeAdoId VARCHAR(255), IterationAdoId int, IterationAdoIdentifier VARCHAR(255), TeamAdoId VARCHAR(255), Days int, IsDev bit);";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#DevCapacity";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("EmployeeAdoId", typeof(string));
                    dataTable.Columns.Add("IterationAdoId", typeof(int));
                    dataTable.Columns.Add("IterationAdoIdentifier", typeof(string));
                    dataTable.Columns.Add("TeamAdoId", typeof(string));
                    dataTable.Columns.Add("Days", typeof(int));
                    dataTable.Columns.Add("IsDev", typeof(bool));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.EmployeeAdoId,
                            item.IterationAdoId,
                            item.IterationAdoIdentifier,
                            item.TeamAdoId,
                            item.Days,
                            item.IsDev
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                // Insert each product into the database
                var mergeQuery = @"
                    MERGE INTO DevCapacity AS target
                    USING (SELECT EmployeeAdoId, IterationAdoId, IterationAdoIdentifier, TeamAdoId, Days, IsDev FROM #DevCapacity) AS source
                    ON target.EmployeeAdoId = source.EmployeeAdoId
                    AND target.IterationAdoIdentifier = source.IterationAdoIdentifier
                    AND target.TeamAdoId = source.TeamAdoId
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.EmployeeAdoId = source.EmployeeAdoId,
                            target.IterationAdoId = source.IterationAdoId,
                            target.IterationAdoIdentifier = source.IterationAdoIdentifier,
                            target.TeamAdoId = source.TeamAdoId,
                            target.Days = source.Days,
                            target.IsDev = source.IsDev
                    WHEN NOT MATCHED THEN
                        INSERT (EmployeeAdoId, IterationAdoId, IterationAdoIdentifier, TeamAdoId, Days, IsDev)
                        VALUES (source.EmployeeAdoId, source.IterationAdoId, source.IterationAdoIdentifier, source.TeamAdoId, source.Days, source.IsDev);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddEmployees(List<TeamMemberDto> employees)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                // Create a temporary table for Employees
                var sql = @"
                CREATE TABLE #TempEmployees (
                    EmployeeAdoId VARCHAR(255),
                    Name VARCHAR(255),
                    TeamName VARCHAR(255),
                    IsLead BIT,
                    IsFTE BIT,
                    Activity VARCHAR(10) NULL,
                    BCE DECIMAL(28,12),
                    Rating TINYINT
                );";
                connection.Execute(sql);

                // Use SqlBulkCopy to insert data into the temporary table
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#TempEmployees";

                    // Create a DataTable to hold the data
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("EmployeeAdoId", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("TeamName", typeof(string));
                    dataTable.Columns.Add("IsLead", typeof(bool));
                    dataTable.Columns.Add("IsFTE", typeof(bool));
                    dataTable.Columns.Add("Activity", typeof(string));
                    dataTable.Columns.Add("BCE", typeof(decimal));
                    dataTable.Columns.Add("Rating", typeof(byte));

                    // Populate the DataTable with data
                    foreach (var employee in employees)
                    {
                        dataTable.Rows.Add(
                            employee.EmployeeAdoId,
                            employee.Name,
                            employee.TeamName,
                            employee.IsLead,
                            employee.IsFTE,
                            employee.Activity,
                            employee.BCE,
                            employee.Rating
                        );
                    }

                    // Write data to the temporary table
                    bulkCopy.WriteToServer(dataTable);
                }

                // Merge the data from the temporary table into the Employees table
                var mergeQuery = @"
                    MERGE INTO Employees AS target
                    USING (SELECT EmployeeAdoId, Name, TeamName, IsLead, IsFTE, Activity, BCE, Rating FROM #TempEmployees) AS source
                    ON target.EmployeeAdoId = source.EmployeeAdoId
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.Name = source.Name,
                            target.TeamName = source.TeamName,
                            target.IsLead = source.IsLead,
                            target.IsFTE = source.IsFTE,
                            target.Activity = source.Activity,
                            target.BCE = source.BCE,
                            target.Rating = source.Rating
                    WHEN NOT MATCHED THEN
                        INSERT (EmployeeAdoId, Name, TeamName, IsLead, IsFTE, Activity, BCE, Rating)
                        VALUES (source.EmployeeAdoId, source.Name, source.TeamName, source.IsLead, source.IsFTE, source.Activity, source.BCE, source.Rating);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddEmployees2(List<TeamMemberDto> employees)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                // Create a temporary table for Employees
                var sql = @"
                CREATE TABLE #TempEmployees2 (
                    EmployeeAdoId VARCHAR(255),
                    Name VARCHAR(255)
                );";
                connection.Execute(sql);

                // Use SqlBulkCopy to insert data into the temporary table
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#TempEmployees2";

                    // Create a DataTable to hold the data
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("EmployeeAdoId", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));

                    // Populate the DataTable with data
                    foreach (var employee in employees)
                    {
                        dataTable.Rows.Add(
                            employee.EmployeeAdoId,
                            employee.Name
                        );
                    }

                    // Write data to the temporary table
                    bulkCopy.WriteToServer(dataTable);
                }

                // Merge the data from the temporary table into the Employees table
                var mergeQuery = @"
                    MERGE INTO Employees2 AS target
                    USING (SELECT EmployeeAdoId, Name FROM #TempEmployees2) AS source
                    ON target.EmployeeAdoId = source.EmployeeAdoId
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.Name = source.Name                            
                    WHEN NOT MATCHED THEN
                        INSERT (EmployeeAdoId, Name, IsFTE, IsLead)
                        VALUES (source.EmployeeAdoId, source.Name, 1, 0);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddIterations(List<IterationDto> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #Iterations (Id int, Identifier VARCHAR(255), Name VARCHAR(255), Path VARCHAR(255), StartDate datetime, EndDate datetime, YearQuarter VARCHAR(255));";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#Iterations";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("Id", typeof(int));
                    dataTable.Columns.Add("Identifier", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Path", typeof(string));
                    dataTable.Columns.Add("StartDate", typeof(DateTime));
                    dataTable.Columns.Add("EndDate", typeof(DateTime));
                    dataTable.Columns.Add("YearQuarter", typeof(string));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.Id,
                            item.Identifier,
                            item.Name,
                            item.Path,
                            item.StartDate,
                            item.EndDate,
                            item.YearQuarter
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO Iterations AS target
                            USING (SELECT Id, Identifier, Name, Path, StartDate, EndDate, YearQuarter FROM #Iterations) AS source
                            ON target.Id = source.Id
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.Identifier = source.Identifier,
                                    target.Name = source.Name,
                                    target.Path = source.Path,
                                    target.StartDate = source.StartDate,
                                    target.EndDate = source.EndDate,
                                    target.YearQuarter = source.YearQuarter
                            WHEN NOT MATCHED THEN
                                INSERT (Id, Identifier, Name, Path, StartDate, EndDate, YearQuarter)
                                VALUES (source.Id, source.Identifier, source.Name, source.Path, source.StartDate, source.EndDate, source.YearQuarter);";
                
                connection.Execute(mergeQuery);               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddTeams(List<Team> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #Teams (Id VARCHAR(255), Name VARCHAR(255), AreaName VARCHAR(255));";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#Teams";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("Id", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("AreaName", typeof(string));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.Id,
                            item.Name,
                            item.AreaName
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO Teams AS target
                            USING (
                                SELECT T.Id, T.Name, A.Id as AreaId 
                                FROM #Teams T
                                JOIN Areas A ON A.Name = T.AreaName
                            ) AS source
                            ON target.Id = source.Id
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.Name = source.Name,
                                    target.AreaId = source.AreaId
                            WHEN NOT MATCHED THEN
                                INSERT (Id, Name, AreaId)
                                VALUES (source.Id, source.Name, source.AreaId);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddWorkItemTags(List<WorkItemTag> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #WorkItemTags (WorkItemId int, Tag VARCHAR(255));";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#WorkItemTags";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("WorkItemId", typeof(int));
                    dataTable.Columns.Add("Tag", typeof(string));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.WorkItemId,
                            item.Tag
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO WorkItemTags AS target
                            USING (
                                SELECT T.WorkItemId, T.Tag
                                FROM #WorkItemTags T
                            ) AS source
                            ON target.WorkItemId = source.WorkItemId AND target.Tag = source.Tag                           
                            WHEN NOT MATCHED THEN
                                INSERT (WorkItemId, Tag)
                                VALUES (source.WorkItemId, source.Tag);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddAreas(List<Area> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #Areas (Id int, Name VARCHAR(255));";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#Areas";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("Id", typeof(int));
                    dataTable.Columns.Add("Name", typeof(string));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.Id,
                            item.Name
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO Areas AS target
                            USING (
                                SELECT Id, Name
                                FROM #Areas
                            ) AS source
                            ON target.Id = source.Id
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.Name = source.Name
                            WHEN NOT MATCHED THEN
                                INSERT (Id, Name)
                                VALUES (source.Id, source.Name);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddUpdateWorkItems(List<WorkItemDetailsDto> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            var dataDistinct = data.Distinct();
            
            try
            {
                var sql = "CREATE TABLE #WorkItems (WorkItemId int NOT NULL, EmployeeAdoId VARCHAR(255), IterationPath VARCHAR(255), IterationId int, WorkItemType VARCHAR(20), Estimate DECIMAL(28,12), Remaining DECIMAL(28,12), AreaAdoId int, ParentType varchar(50), IsDone bit, Activity varchar(50));";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#WorkItems";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("WorkItemId", typeof(int));
                    dataTable.Columns.Add("EmployeeAdoId", typeof(string));
                    dataTable.Columns.Add("IterationPath", typeof(string));
                    dataTable.Columns.Add("IterationId", typeof(int));
                    dataTable.Columns.Add("WorkItemType", typeof(string));
                    dataTable.Columns.Add("Estimate", typeof(decimal));
                    dataTable.Columns.Add("Remaining", typeof(decimal));
                    dataTable.Columns.Add("AreaAdoId", typeof(int));
                    dataTable.Columns.Add("ParentType", typeof(string));
                    dataTable.Columns.Add("IsDone", typeof(bool));
                    dataTable.Columns.Add("Activity", typeof(string));

                    foreach (var item in dataDistinct)
                    {
                        dataTable.Rows.Add(
                            item.WorkItemId,
                            item.EmployeeAdoId,
                            item.IterationPath,
                            item.IterationId,
                            item.WorkItemType,
                            item.Estimate,
                            item.Remaining,
                            item.AreaAdoId,
                            item.ParentType,
                            item.IsDone,
                            item.Activity
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO WorkItems AS target
                            USING (SELECT WorkItemId, EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, Remaining, AreaAdoId, ParentType, IsDone, Activity FROM #WorkItems) AS source
                            ON target.WorkItemId = source.WorkItemId
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.EmployeeAdoId = source.EmployeeAdoId,
                                    target.IterationPath = source.IterationPath,
                                    target.IterationId = source.IterationId,
                                    target.WorkItemType = source.WorkItemType,
                                    target.Estimate = source.Estimate,
                                    target.AreaAdoId = source.AreaAdoId,
                                    target.ParentType = source.ParentType,
                                    target.Remaining = source.Remaining,
                                    target.IsDone = source.IsDone,
                                    target.Activity = source.Activity
                            WHEN NOT MATCHED THEN
                                INSERT (WorkItemId, EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, Remaining, AreaAdoId, ParentType, IsDone, Activity)
                                VALUES (source.WorkItemId, source.EmployeeAdoId, source.IterationPath, source.IterationId, source.WorkItemType, source.Estimate, source.Remaining, source.AreaAdoId, source.ParentType, source.IsDone, source.Activity);";
                
                connection.Execute(mergeQuery);               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal static void AddUpdateWorkItemsPlannedDone(List<WorkItemPlannedData> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #WorkItemsPlannedDone(WorkItemId int, EmployeeAdoId VARCHAR(255), IterationId int, AreaAdoId int, IsDone bit, IsPlanned bit, IsDeleted bit, IsRemovedFromSprint bit);";
                connection.Execute(sql);

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#WorkItemsPlannedDone";

                    // Write data to the server
                    using var dataTable = new DataTable();
                    dataTable.Columns.Add("WorkItemId", typeof(int));
                    dataTable.Columns.Add("EmployeeAdoId", typeof(string));
                    dataTable.Columns.Add("IterationId", typeof(int));
                    dataTable.Columns.Add("AreaAdoId", typeof(int));
                    dataTable.Columns.Add("IsDone", typeof(bool));
                    dataTable.Columns.Add("IsPlanned", typeof(bool));
                    dataTable.Columns.Add("IsDeleted", typeof(bool));
                    dataTable.Columns.Add("IsRemovedFromSprint", typeof(bool));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.WorkItemId,
                            item.EmployeeAdoId,
                            item.IterationId,
                            item.AreaAdoId,
                            item.IsDone,
                            item.IsPlanned,
                            item.IsDeleted,
                            item.IsRemovedFromSprint
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO WorkItemsPlannedDone AS target
                            USING (SELECT WorkItemId, EmployeeAdoId, IterationId, AreaAdoId, IsDone, IsPlanned, IsDeleted, IsRemovedFromSprint FROM #WorkItemsPlannedDone) AS source
                            ON target.WorkItemId = source.WorkItemId
                            AND target.IterationId = source.IterationId
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.EmployeeAdoId = source.EmployeeAdoId,
                                    target.IterationId = source.IterationId,
                                    target.AreaAdoId = source.AreaAdoId,
                                    target.IsDone = source.IsDone,
                                    target.IsDeleted = source.IsDeleted,
                                    target.IsPlanned = source.IsPlanned,
                                    target.IsRemovedFromSprint = source.IsRemovedFromSprint
                            WHEN NOT MATCHED THEN
                                INSERT (WorkItemId, EmployeeAdoId, IterationId, AreaAdoId, IsDone, IsPlanned, IsDeleted, IsRemovedFromSprint)
                                VALUES (source.WorkItemId, source.EmployeeAdoId, source.IterationId, source.AreaAdoId, source.IsDone, source.IsPlanned, source.IsDeleted, source.IsRemovedFromSprint);";

                connection.Execute(mergeQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
