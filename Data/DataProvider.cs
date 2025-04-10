using System.Data.SqlClient;
using ADOExport.Models;
using ADOExport.Services;
using Dapper;

namespace ADOExport.Data
{
    public static class SqlDataProvider
    {
        private static string ConnectionString = SettingsService.CurrentSettings.ConnectionString;

        public static void AddAllData(List<WorkItemDetailsDto> data)
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
                        var sql = "INSERT INTO AllData (EmployeeAdoId, IterationPath, IterationId, WorkItemType, Estimate, AreaAdoId) VALUES (@EmployeeAdoId, @IterationPath, @IterationId, @WorkItemType, @Estimate, @AreaAdoId)";
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
                        // Insert each product into the database
                        var sql = "INSERT INTO Employees (EmployeeAdoId, Name, IsLead, IsFTE, Activity, TeamName, BCE, Rating) VALUES (@EmployeeAdoId, @Name, @IsLead, @IsFTE, @Activity, @TeamName, @BCE, @Rating)";
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

        public static void ClearData()
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
                        var sql = "DELETE FROM AllData; DELETE FROM DevCapacity; DELETE FROM Employees; DELETE FROM iterations;";
                        connection.Execute(sql, transaction: transaction);

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
