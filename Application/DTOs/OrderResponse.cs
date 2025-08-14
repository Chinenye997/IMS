
namespace Application.DTOs
{
    // Data to show order details (e.g., in history or details view)
    public class OrderResponse
    {
        // Order ID (your "OrderId")
        public string Id { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        // When the order was placed
        public DateTime OrderDate { get; set; }
        // Total cost of all items
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        // List of items in the order
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }

    // Data from the payment popup form (user selects payment method)
    public class OrderRequest
    {
        // Payment method chosen (Cash or Transfer)
        public string PaymentMethod { get; set; } = "Cash";
    }

    // Details of each item in an order
    public class OrderItemResponse
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        // Product thumbnail URL (if any)
        public string? PhotoUrl { get; set; }
        // Number of units purchased
        public int Quantity { get; set; }
        // Price per unit
        public decimal UnitPrice { get; set; }
        // Total for this item (Quantity * UnitPrice)
        public decimal Subtotal { get; set; }
    }
}
