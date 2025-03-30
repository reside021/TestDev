namespace TestDevicon.Server.Models.Entities
{
    public class Valute
    {
        public int Id { get; set; }
        public string ValuteCode { get; set; }
        public string CharCode { get; set; }
        public ICollection<ExchangeRate> ExchangeRates { get; set; }
    }
}
