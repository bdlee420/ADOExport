using ADOExport.Models;

namespace ADOExport.Services
{
    internal class AreaService
    {
        internal async static Task<List<Area>> GetAreasAsync()
        {
            var areas = await ADOService.GetAreas();           

            Console.WriteLine($"Get Areas Count = {areas.Count}");

            return areas;
        }
    }
}
