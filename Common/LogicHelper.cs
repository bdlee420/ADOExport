namespace ADOExport.Common
{
    internal static class LogicHelper
    {
        internal static string GetYearQuarter(string name, DateTime start, DateTime end)
        {
            if (end.Month == 4 && name.Contains("End of"))
                return $"{end.Year}-Q1";
            else if (end.Month == 7 && name.Contains("End of"))
                return $"{end.Year}-Q2";
            else if (end.Month == 10 && name.Contains("End of"))
                return $"{end.Year}-Q3";
            else if (end.Month == 1 && name.Contains("End of"))
                return $"{start.Year}-Q4";
            else if (end.Month < 4)
                return $"{end.Year}-Q1";
            else if (end.Month > 3 && end.Month <= 6)
                return $"{end.Year}-Q2";
            else if (end.Month > 6 && end.Month <= 9)
                return $"{end.Year}-Q3";
            else if (end.Month > 9 && end.Month <= 12)
                return $"{end.Year}-Q4";
            else
                Console.WriteLine($"Unhandled quarter conversion: {name} {start} {end}");
            throw new Exception("Unhandled quarter conversion");
        }
        internal static int CountBusinessDays(DateTime start, DateTime end)
        {
            // Ensure start is before end
            if (start > end)
            {
                throw new ArgumentException("Start date must be earlier than end date.");
            }

            int businessDays = 0;

            // Iterate through each date in the range
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                // Check if the day is a weekday (Monday to Friday)
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
            }

            return businessDays;
        }
    }
}
