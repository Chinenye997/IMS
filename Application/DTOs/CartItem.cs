namespace Application.DTOs
{
    public class CartItem
    {
        // Matches ProductResponse.Id
        public string ProductId { get; set; } // Unique product key
        public string ProductName { get; set; }
        public string PhotoUrl { get; set; } // Thumbnail URL
        public decimal UnitPrice { get; set; } // Price per unit
        // Quantity user wants to purchase
        public int Quantity { get; set; } // Number of units
    }
}