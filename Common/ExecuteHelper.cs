using System.Diagnostics;

namespace ADOExport.Common
{
    internal class ExecuteHelper
    {
        internal async static Task<T> ExecuteAndLogAction<T>(Stopwatch stopwatch, string actionName, Func<Task<T>> action)
        {
            Console.WriteLine($"{actionName} Start");
            long elapsedStart = stopwatch.ElapsedMilliseconds;

            var result = await action.Invoke();

            Console.WriteLine($"{actionName} Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            return result;
        }

        internal static T ExecuteAndLogAction<T>(Stopwatch stopwatch, string actionName, Func<T> action)
        {
            Console.WriteLine($"{actionName} Start");
            long elapsedStart = stopwatch.ElapsedMilliseconds;

            T result = action.Invoke();

            Console.WriteLine($"{actionName} Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");

            return result;
        }

        internal static void ExecuteAndLogAction(Stopwatch stopwatch, string actionName, Action action)
        {
            Console.WriteLine($"{actionName} Start");
            long elapsedStart = stopwatch.ElapsedMilliseconds;

            action.Invoke();

            Console.WriteLine($"{actionName} Finished: {stopwatch.ElapsedMilliseconds - elapsedStart}ms");
            Console.WriteLine("");
        }
    }
}
