using Microsoft.AspNetCore.Mvc;
using TestDevicon.Server.Services;

namespace TestDevicon.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeRatesController : Controller
    {
        private readonly IRatesService _ratesService;
        private readonly ILogger<ExchangeRatesController> _logger;

        public ExchangeRatesController(IRatesService ratesService, ILogger<ExchangeRatesController> logger)
        {
            _ratesService = ratesService;
            _logger = logger;
        }

        [HttpGet("update")]
        public async Task<IActionResult> UpdateDataRates()
        {
            try
            {
                var success = await _ratesService.LoadExchangeRates();
                if (!success)
                {
                    return BadRequest("Не удалось обновить данные");
                }
                var dataRates = await _ratesService.GetLastExchangeRates();
                return Ok(dataRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ручном обнволении котировок!");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("period")]
        public async Task<IActionResult> GetDataRatesFromPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Неверный период!");
                }
                var dataRates = await _ratesService.GetExchangeRates(startDate, endDate);
                return Ok(dataRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении котировок за период!");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}
