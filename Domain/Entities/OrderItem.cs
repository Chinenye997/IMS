
namespace Domain.Entities
{
    // Represents one product in an order
    public class OrderItem
    {
        // Unique ID for the order item
        public string Id { get; set; } = Guid.NewGuid().ToString();
        // Links to the parent order
        public string OrderId { get; set; } = string.Empty;
        // Navigation property to Order
        public Order? Order { get; set; }
        // Links to the product purchased
        public string ProductId { get; set; } = string.Empty;
        // Navigation property to Product
        public ProductEntity? Product { get; set; }
        // Number of units purchased
        public int Quantity { get; set; }
        // Price per unit at purchase time
        public decimal UnitPrice { get; set; }
        // Total for this item (Quantity * UnitPrice)
        public decimal Subtotal { get; set; }
    }
}
