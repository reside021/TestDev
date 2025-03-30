using TestDevicon.Server.Models.DTOs;

namespace TestDevicon.Server.Services
{
    public interface IRatesService
    {
        Task<Dictionary<string,List<ExchangeRateDto>>> GetExchangeRates(DateOnly startDate, DateOnly endDate);
        Task<Dictionary<string,List<ExchangeRateDto>>> GetLastExchangeRates();
        Task<bool> LoadExchangeRates(bool isNeedLoadLastMonth = false);
    }
}
