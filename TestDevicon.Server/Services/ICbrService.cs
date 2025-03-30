using TestDevicon.Server.Models.DTOs;

namespace TestDevicon.Server.Services
{
    public interface ICbrService
    {
        Task<List<ValuteDto>> GetDataRates(DateOnly date);
        Task<List<ValuteDto>> GetDataRatesInRange(DateOnly startDate, DateOnly endDate, string valuteCode);
    }
}
