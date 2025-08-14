
namespace Application.DTOs
{
    public class Finances
    {
        public decimal TotalStockValue { get; set; }
        public List<MostSoldDto> TopSellers { get; set; } = new();
    }
}
