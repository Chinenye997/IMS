
namespace Domain.Entities
{
    // Represents a cohase (like a receipt)mplete purc
    public class Order
    {
        // Unique order ID (your "OrderId")
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceNo { get; set; } = string.Empty;
        // Links to the customer (ASP.NET Identity user)
        public string UserId { get; set; } = string.Empty;
        // Navigation property to UserEntity
        public UserEntity? User { get; set; }
        // When the order was placed
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        // Total cost of all items
        public decimal TotalAmount { get; set; }
        // Payment method (Cash or Transfer)
        public string PaymentMethod { get; set; } = "Cash";
        // Payment status (Pending or Completed)
        public string PaymentStatus { get; set; } = "Pending";
        // When payment was completed
        public DateTime? PaidAt { get; set; }
        // List of items in the order
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
