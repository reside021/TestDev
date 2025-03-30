using Microsoft.EntityFrameworkCore;
using TestDevicon.Server.Data;
using TestDevicon.Server.Services;

namespace TestDevicon.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Логгирование
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddFile("Logs/log-{Date}.txt");


            // Подключение к БД
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );


            builder.Services.AddScoped<ICbrService, CbrService>();
            builder.Services.AddScoped<IRatesService, RatesService>();

            builder.Services.AddHostedService<ExchangeRatesBackgroundService>();

            // Список необходимых валют
            var valutes = builder.Configuration.GetSection("ValuteCodes").Get<string[]>();
            if (valutes is not null)
            {
                builder.Services.AddSingleton(valutes);
            }

            var app = builder.Build();

            
            using var scope = app.Services.CreateAsyncScope();
            try
            {
                // Инициализация БД
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();

                // Автозагрузка за последний месяц
                var ratesService = scope.ServiceProvider.GetRequiredService<IRatesService>();
                await ratesService.LoadExchangeRates(isNeedLoadLastMonth: true);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ошибка инициализации базы данных");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
