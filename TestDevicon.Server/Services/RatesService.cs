using Microsoft.EntityFrameworkCore;
using TestDevicon.Server.Data;
using TestDevicon.Server.Models.DTOs;
using TestDevicon.Server.Models.Entities;

namespace TestDevicon.Server.Services
{
    public class RatesService : IRatesService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ICbrService _cbrService;
        private readonly ILogger<RatesService> _logger;
        private readonly string[] _valuteCodes;

        public RatesService(AppDbContext appDbContext, ICbrService cbrService, ILogger<RatesService> logger, string[] valuteCodes)
        {
            _appDbContext = appDbContext;
            _cbrService = cbrService;
            _logger = logger;
            _valuteCodes = valuteCodes;
        }

        public async Task<Dictionary<string,List<ExchangeRateDto>>> GetExchangeRates(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    throw new Exception("Неверно задан период дат!");
                }
                var countDays = endDate.DayNumber - startDate.DayNumber + 1;

                var data = await _appDbContext
                    .ExchangeRates
                    .AsNoTracking()
                    .Include(x => x.Valute)
                    .Where(x => x.Date >= startDate && x.Date <= endDate)
                    .ToListAsync();

                var countDaysInData = data
                    .Select(x => x.Date)
                    .Distinct()
                    .Count();

                if (countDays != countDaysInData)
                {
                    var dataRates = new List<ValuteDto>();
                    var valutes = await _appDbContext.Valutes.AsNoTracking().Select(x => x.ValuteCode).ToListAsync();
                    foreach (var valute in valutes)
                    {
                        var dataRate = await _cbrService.GetDataRatesInRange(startDate, endDate, valute);
                        dataRates.AddRange(dataRate);
                    }
                    await SaveDataInDatabase(dataRates);

                    data = await _appDbContext
                    .ExchangeRates
                    .AsNoTracking()
                    .Include(x => x.Valute)
                    .Where(x => x.Date >= startDate && x.Date <= endDate)
                    .ToListAsync();
                }

                var groupedData = data.GroupBy(x => x.Date).ToList();

                var result = new List<ExchangeRateDto>();

                foreach (var date in groupedData)
                {
                    var rates = new Dictionary<string, decimal>();
                    foreach (var rate in date)
                    {
                        rates[rate.Valute.CharCode] = rate.ValutePrice;
                    }
                    result.Add(new ExchangeRateDto()
                    {
                        Date = date.Key,
                        Rates = rates
                    });
                }
                return new Dictionary<string, List<ExchangeRateDto>>
                {
                    { nameof(result), result }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в получении котировок по периоду!");
                throw;
            }
        }

        public async Task<Dictionary<string, List<ExchangeRateDto>>> GetLastExchangeRates()
        {
            var last10Dates = await _appDbContext
                .ExchangeRates
                .AsNoTracking()
                .Select(x => x.Date)
                .Distinct()
                .OrderDescending()
                .Take(10)
                .ToListAsync();

            var data = await _appDbContext
                .ExchangeRates
                .AsNoTracking()
                .Include(x => x.Valute)
                .Where(x => last10Dates.Contains(x.Date))
                .GroupBy(x => x.Date)
                .ToListAsync();

            var result = new List<ExchangeRateDto>();

            foreach (var date in data)
            {
                var rates = new Dictionary<string, decimal>();
                foreach (var rate in date)
                {
                    rates[rate.Valute.CharCode] = rate.ValutePrice;
                }
                result.Add(new ExchangeRateDto()
                {
                    Date = date.Key,
                    Rates = rates
                });
            }
            return new Dictionary<string, List<ExchangeRateDto>>
            {
                { nameof(result), result }
            };
        }

        public async Task<bool> LoadExchangeRates(bool isNeedLoadLastMonth = false)
        {
            try
            {
                await UpdateTodayRates();
                if (isNeedLoadLastMonth)
                {
                    await FillDataForLastMonth();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в загрузке котировок");
                throw;
            }
        }

        private async Task UpdateTodayRates()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var dataRates = await _cbrService.GetDataRates(today);
                await SaveDataInDatabase(dataRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в обновлении данных");
                throw;
            }

        }

        private async Task FillDataForLastMonth()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var beginMonthDate = today.AddMonths(-1);
                var endMonthData = today.AddDays(-1);

                var dataRates = new List<ValuteDto>();
                foreach (var valute in _valuteCodes)
                {
                    var dataRate = await _cbrService.GetDataRatesInRange(beginMonthDate, endMonthData, valute);
                    dataRates.AddRange(dataRate);
                }
                await SaveDataInDatabase(dataRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в заполнении котировок за последний месяц");
                throw;
            }
        }
        private async Task SaveDataInDatabase(List<ValuteDto> dataRates)
        {
            try
            {
                foreach (var dataRate in dataRates)
                {
                    if (_valuteCodes.Length > 0 && !_valuteCodes.Contains(dataRate.ValuteCode))
                        continue;

                    var valute = await _appDbContext
                        .Valutes
                        .FirstOrDefaultAsync(x => x.ValuteCode == dataRate.ValuteCode)
                        ?? new Valute
                        {
                            ValuteCode = dataRate.ValuteCode,
                            CharCode = dataRate.CharCode
                        };

                    var exchangeRate = await _appDbContext
                        .ExchangeRates
                        .FirstOrDefaultAsync(x => x.Date == dataRate.Date && x.ValuteId == valute.Id);

                    if (exchangeRate is null)
                    {
                        await _appDbContext.ExchangeRates.AddAsync(new ExchangeRate()
                        {
                            Date = dataRate.Date,
                            Valute = valute,
                            ValutePrice = dataRate.ValutePrice,
                        });
                    } 
                    else
                    {
                        exchangeRate.ValutePrice = dataRate.ValutePrice;
                    }
                }
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения котировок в базе данных!");
                throw;
            }
        }
    }
}
