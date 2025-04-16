namespace ADOExport.Common
{
    internal static class CommonFunctions
    {
        internal static bool AnySafe<T>(this List<T> that)
        {
            return that?.Count > 0;
        }
    }
}
