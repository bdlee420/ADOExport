using ADOExport.Common;
using ADOExport.Data;
using ADOExport.Models;

namespace ADOExport.Services
{
    internal static class IterationsService
    {      
        internal async static Task<List<IterationDto>> GetIterationsAsync(List<string> requestedIterations)
        {
            List<IterationDto> iterationsDto;
            try
            {
                var iterations = await ADOService.GetIterations();

                iterationsDto = iterations.Select(i => new IterationDto
                {
                    Identifier = i.Id,
                    Name = i.Name,
                    StartDate = i.StartDate,
                    EndDate = i.FinishDate,
                    Path = i.FriendlyPath,
                    Id = i.NodeId,
                    YearQuarter = LogicHelper.GetYearQuarter(i.Name, i.StartDate, i.FinishDate)
                }).ToList();                
            }
            catch
            {
                iterationsDto = await SqlDataProvider.GetIterationsAsync();
            }

            if (requestedIterations.Count > 0)
            {
                return iterationsDto
                        .Where(i => requestedIterations.Any(iteration => i.Name.EndsWith(iteration)))
                        .ToList();
            }

            return iterationsDto;
        }
    }
}
