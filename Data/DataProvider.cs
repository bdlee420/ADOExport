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
                var sql = "CREATE TABLE #DevCapacity (EmployeeAdoId VARCHAR(255), IterationAdoId int, IterationAdoIdentifier VARCHAR(255), TeamAdoId VARCHAR(255), Days int);";
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

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.EmployeeAdoId,
                            item.IterationAdoId,
                            item.IterationAdoIdentifier,
                            item.TeamAdoId,
                            item.Days
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                // Insert each product into the database
                var mergeQuery = @"
                    MERGE INTO DevCapacity AS target
                    USING (SELECT EmployeeAdoId, IterationAdoId, IterationAdoIdentifier, TeamAdoId, Days FROM #DevCapacity) AS source
                    ON target.EmployeeAdoId = source.EmployeeAdoId
                    AND target.IterationAdoIdentifier = source.IterationAdoIdentifier
                    AND target.TeamAdoId = source.TeamAdoId
                    WHEN MATCHED THEN
                        UPDATE SET
                            target.EmployeeAdoId = source.EmployeeAdoId,
                            target.IterationAdoId = source.IterationAdoId,
                            target.IterationAdoIdentifier = source.IterationAdoIdentifier,
                            target.TeamAdoId = source.TeamAdoId,
                            target.Days = source.Days
                    WHEN NOT MATCHED THEN
                        INSERT (EmployeeAdoId, IterationAdoId, IterationAdoIdentifier, TeamAdoId, Days)
                        VALUES (source.EmployeeAdoId, source.IterationAdoId, source.IterationAdoIdentifier, source.TeamAdoId, source.Days);";

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

        internal static void AddUpdateWorkItems(List<WorkItemDetailsDto> data)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Open();

            try
            {
                var sql = "CREATE TABLE #WorkItems (WorkItemId int NOT NULL, EmployeeAdoId VARCHAR(255), IterationPath VARCHAR(255), IterationId int, WorkItemType VARCHAR(20), Estimate DECIMAL(28,12), AreaAdoId int);";
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
                    dataTable.Columns.Add("AreaAdoId", typeof(int));

                    foreach (var item in data)
                    {
                        dataTable.Rows.Add(
                            item.WorkItemId,
                            item.EmployeeAdoId,
                            item.IterationPath,
                            item.IterationId,
                            item.WorkItemType,
                            item.Estimate,
                            item.AreaAdoId
                        );
                    }

                    bulkCopy.WriteToServer(dataTable);
                }

                var mergeQuery = @"
                            MERGE INTO WorkItems AS target
                            USING (SELECT WorkItemId, EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, AreaAdoId FROM #WorkItems) AS source
                            ON target.WorkItemId = source.WorkItemId
                            WHEN MATCHED THEN
                                UPDATE SET
                                    target.EmployeeAdoId = source.EmployeeAdoId,
                                    target.IterationPath = source.IterationPath,
                                    target.IterationId = source.IterationId,
                                    target.WorkItemType = source.WorkItemType,
                                    target.Estimate = source.Estimate,
                                    target.AreaAdoId = source.AreaAdoId
                            WHEN NOT MATCHED THEN
                                INSERT (WorkItemId, EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, AreaAdoId)
                                VALUES (source.WorkItemId, source.EmployeeAdoId, source.IterationPath, source.IterationId, source.WorkItemType, source.Estimate, source.AreaAdoId);";
                
                connection.Execute(mergeQuery);               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }       
    }
}
