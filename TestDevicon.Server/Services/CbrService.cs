using System.Xml.Linq;
using TestDevicon.Server.Models.DTOs;

namespace TestDevicon.Server.Services
{
    public class CbrService : ICbrService
    {
        private readonly ILogger<CbrService> _logger;

        public CbrService(ILogger<CbrService> logger)
        {
            _logger = logger;
        }

        public async Task<List<ValuteDto>> GetDataRates(DateOnly date)
        {
            try
            {
                var url = $"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date:dd/MM/yyyy}";
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(url);

                var data = XDocument.Parse(response);

                var dataRates = new List<ValuteDto>();

                foreach (var valute in data.Descendants("Valute"))
                {
                    string id = valute.Attribute("ID")?.Value;
                    string charCode = valute.Element("CharCode")?.Value;
                    string valutePrice = valute.Element("Value")?.Value;

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(charCode) && decimal.TryParse(valutePrice, out var price))
                    {
                        dataRates.Add(new ValuteDto()
                        {
                            ValuteCode = id,
                            CharCode = charCode,
                            ValutePrice = price,
                            Date = date
                        });
                    }
                }
                return dataRates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка в запросе на поулчение котировок [{date:dd/MM/yyyy}]");
                throw;
            }
        }

        public async Task<List<ValuteDto>> GetDataRatesInRange(DateOnly startDate, DateOnly endDate, string valuteCode)
        {
            try
            {
                var url = $"https://cbr.ru/scripts/XML_dynamic.asp?date_req1={startDate:dd/MM/yyyy}&date_req2={endDate:dd/MM/yyyy}&VAL_NM_RQ={valuteCode}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(url);

                var data = XDocument.Parse(response);

                var dataRates = new List<ValuteDto>();

                foreach (var record in data.Descendants("Record"))
                {
                    string date = record.Attribute("Date")?.Value;
                    string valutePrice = record.Element("VunitRate")?.Value;

                    if (DateOnly.TryParse(date, out var datePrice) && decimal.TryParse(valutePrice, out var price))
                    {
                        dataRates.Add(new ValuteDto()
                        {
                            ValuteCode = valuteCode,
                            Date = datePrice,
                            ValutePrice = price,
                        });
                    }
                }
                return dataRates;
            }
            catch (Exception ex)
            {
                string message = $"Ошибка в запросе на поулчение котировок [{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}]";
                _logger.LogError(ex, message);
                throw;
            }
        }
    }
}
