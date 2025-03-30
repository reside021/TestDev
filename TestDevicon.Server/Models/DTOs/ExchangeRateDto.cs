namespace TestDevicon.Server.Models.DTOs
{
    public record ExchangeRateDto
    {
        public DateOnly Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
