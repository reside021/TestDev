namespace TestDevicon.Server.Services
{
    public class ExchangeRatesBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExchangeRatesBackgroundService> _logger;

        public ExchangeRatesBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ExchangeRatesBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nowTimeStamp = DateTime.Now;
                var nextUpdateTime = nowTimeStamp.Date.AddHours(1);

                if (nowTimeStamp > nextUpdateTime)
                {
                    nextUpdateTime = nextUpdateTime.AddDays(1);
                }

                var delay = nextUpdateTime - nowTimeStamp;

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateAsyncScope();

                    var ratesService = scope.ServiceProvider.GetRequiredService<IRatesService>();

                    await ratesService.LoadExchangeRates();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в фоновом сервисе обновления котировок!");
                }
            }
        }
    }
}
