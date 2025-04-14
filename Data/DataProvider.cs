using System.Data;
using System.Data.SqlClient;
using ADOExport.Models;
using ADOExport.Services;
using Dapper;

namespace ADOExport.Data
{
    public static class SqlDataProvider
    {
        private static string ConnectionString = SettingsService.CurrentSettings.ConnectionString;

        public static void AddWorkItems(List<WorkItemDetailsDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert each product into the database
                        var sql = "INSERT INTO WorkItems (WorkItemId, EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, AreaAdoId) VALUES (@WorkItemId, @EmployeeAdoId, @IterationPath, @IterationId, @WorkItemType, @Estimate, @AreaAdoId)";
                        connection.Execute(sql, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public static void AddCapacities(List<CapacityDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert each product into the database
                        var sql = "INSERT INTO DevCapacity (EmployeeAdoId, IterationAdoId, IterationAdoIdentifier, TeamAdoId, Days) VALUES (@EmployeeAdoId, @IterationAdoId, @IterationAdoIdentifier, @TeamAdoId, @Days)";
                        connection.Execute(sql, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public static void AddEmployees(List<TeamMemberDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var sql = "DROP TABLE IF EXISTS #Employees; CREATE TABLE #Employees (EmployeeAdoId VARCHAR(255), Name VARCHAR(255), TeamName VARCHAR(255), IsLead bit, IsFTE bit, Activity varchar(10) NULL, BCE decimal(28,12), Rating tinyint)";
                        connection.Execute(sql, data, transaction: transaction);

                        sql = "INSERT INTO #Employees (EmployeeAdoId, Name, IsLead, IsFTE, Activity, TeamName, BCE, Rating) VALUES (@EmployeeAdoId, @Name, @IsLead, @IsFTE, @Activity, @TeamName, @BCE, @Rating)";
                        connection.Execute(sql, data, transaction: transaction);

                        // Insert each product into the database
                        var mergeQuery = @"
                            MERGE INTO Employees AS target
                            USING (SELECT EmployeeAdoId, Name, IsLead, IsFTE, Activity, TeamName, BCE, Rating FROM #Employees) AS source
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
                        connection.Execute(mergeQuery, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public static void AddIterations(List<IterationDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert each product into the database
                        var sql = "INSERT INTO Iterations (Id, Identifier, Name, Path, StartDate, EndDate, YearQuarter) VALUES (@Id, @Identifier, @Name, @Path, @StartDate, @EndDate, @YearQuarter)";
                        connection.Execute(sql, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public async static Task<List<IterationDto>> GetIterationsAsync()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var sql = "SELECT Id, Identifier, Name, Path, StartDate, EndDate, YearQuarter FROM Iterations";
                var data = await connection.QueryAsync<IterationDto>(sql);
                return data.ToList();
            }
        }

        public static void ClearWorkItems(List<WorkItemDetailsDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                try
                {
                    var sql = "CREATE TABLE #WorkItems (WorkItemId int);";
                    connection.Execute(sql);

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = "#WorkItems";

                        // Map the properties of the WorkItems class to the corresponding columns in the database
                        bulkCopy.ColumnMappings.Add("WorkItemId", "WorkItemId");

                        // Write data to the server
                        using (var dataTable = new DataTable())
                        {
                            dataTable.Columns.Add("WorkItemId", typeof(int));

                            foreach (var item in data)
                            {
                                dataTable.Rows.Add(item.WorkItemId);
                            }

                            bulkCopy.WriteToServer(dataTable);
                        }
                    }

                    sql = "Delete W FROM WorkItems W JOIN #WorkItems t ON W.WorkItemId = t.WorkItemId";
                    connection.Execute(sql, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        public static void ClearIterations(List<IterationDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var sql = "CREATE TABLE #Iterations (Id int)";
                        connection.Execute(sql, transaction: transaction);

                        sql = "INSERT INTO #Iterations (Id) VALUES (@Id)";
                        connection.Execute(sql, data, transaction: transaction);

                        sql = "Delete I FROM Iterations I JOIN #Iterations t ON I.id = t.Id";
                        connection.Execute(sql, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        public static void ClearDevCapacity(List<IterationDto> data)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Use a transaction for better performance when inserting multiple records
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var sql = "CREATE TABLE #Iterations (Id int)";
                        connection.Execute(sql, transaction: transaction);

                        sql = "INSERT INTO #Iterations (Id) VALUES (@Id)";
                        connection.Execute(sql, data, transaction: transaction);

                        sql = "Delete DC FROM DevCapacity DC JOIN #Iterations I ON I.id = DC.IterationAdoId";
                        connection.Execute(sql, data, transaction: transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }
    }
}
