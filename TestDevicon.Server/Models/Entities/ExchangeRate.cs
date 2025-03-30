namespace TestDevicon.Server.Models.Entities
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int ValuteId { get; set; }
        public Valute Valute { get; set; }
        public decimal ValutePrice { get; set; }
    }
}
