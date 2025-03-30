namespace TestDevicon.Server.Models.DTOs
{
    public class ValuteDto
    {
        public string ValuteCode { get; set; }
        public string CharCode { get; set; }
        public decimal ValutePrice { get; set; }
        public DateOnly Date { get; set; }
    }
}
