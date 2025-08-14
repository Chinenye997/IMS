
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interface
{
    // Defines methods for handling orders
    public interface IPaymentInterface
    {
            // Process a cart checkout and create an order
        Task<bool> ProcessOrderAsync(string userId, OrderRequest request, List<CartItem> cart);

        // Get all orders (for history view)
        Task<List<OrderResponse>> GetAllOrdersAsync(string search = "");

        // Get details of a specific order (for details view)
        Task<OrderResponse?> GetOrderDetailsAsync(string invoiceNo);

        void ClearCart(ISession session); // Add this if not present

       
    }
}
