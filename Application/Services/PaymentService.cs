using Application.DTOs;
using Application.Interface;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    // Handles order processing and retrieval
    public class PaymentService : IPaymentInterface
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContext;

        public PaymentService(AppDbContext db, IHttpContextAccessor httpContext)
        {
            _db = db;
            _httpContext = httpContext;
        }

        // Creates an order from the cart and processes payment (Cash/Transfer)
        public async Task<bool> ProcessOrderAsync(string userId, OrderRequest request, List<CartItem> cart)
        {
            // Check if cart is valid
            if (cart == null || !cart.Any())
                return false;

            // Start a transaction to ensure all changes save or rollback
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Calculate total and validate stock
                decimal totalAmount = 0;
                foreach (var item in cart)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    if (product == null || item.Quantity > product.Quantity)
                        return false; // Invalid product or insufficient stock
                    totalAmount += item.Quantity * product.Price;
                }

                // Generate InvoiceNo (e.g., InvoiceNo-001)
                var orderCount = await _db.Orders.CountAsync() + 1;
                var invoiceNo = $"InvoiceNo-{orderCount:D3}"; // Pads with zeros (e.g., 001)

                // Create the order
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = "Completed", // Instant for Cash/Transfer
                    PaidAt = DateTime.UtcNow,
                    InvoiceNo = invoiceNo // Assign generated invoice number
                };
                _db.Orders.Add(order);

                // Create order items
                foreach (var item in cart)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Subtotal = item.Quantity * product.Price
                    };
                    _db.OrderItems.Add(orderItem);

                    // Update stock
                    product.Quantity -= item.Quantity;
                }

                // Save all changes
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                // Undo changes if something fails
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Retrieves all orders for history with search 
        public async Task<List<OrderResponse>> GetAllOrdersAsync(string search = "")
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(o =>
                    o.InvoiceNo.ToLower().Contains(search) || // Search by InvoiceNo
                    (o.User != null && o.User.FullName != null && o.User.FullName.ToLower().Contains(search)) || // Search by customer name
                    o.PaymentMethod.ToLower().Contains(search) ||
                    o.PaymentStatus.ToLower().Contains(search) ||
                    o.OrderDate.ToString("g").ToLower().Contains(search)
                );
            }

            return await query.Select(o => new OrderResponse
            {
                Id = o.Id,
                // InvoiceNo = o.InvoiceNo, // Include InvoiceNo
                InvoiceNo = o.InvoiceNo ?? "Unknown", // Fallback if InvoiceNo is null (shouldn't happen)
                //CustomerName = o.User != null ? o.User.FullName : "Unknown",
                CustomerName = o.User != null && !string.IsNullOrEmpty(o.User.FullName) ? o.User.FullName : "Unknown",//i will ask you this later
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod ?? "Unknown",
                PaymentStatus = o.PaymentStatus ?? "Unknown",
                PaidAt = o.PaidAt,
                Items = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    PhotoUrl = oi.Product.PhotoUrls.FirstOrDefault(),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                }).ToList()
            }).ToListAsync();
        }

        // Retrieves details for a specific order by InvoiceNo
        public async Task<OrderResponse?> GetOrderDetailsAsync(string invoiceNo)
        {
            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.InvoiceNo == invoiceNo) // Use InvoiceNo instead of Id
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    InvoiceNo = o.InvoiceNo, // Include InvoiceNo
                    CustomerName = o.User != null ? o.User.FullName : "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    PaidAt = o.PaidAt,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        PhotoUrl = oi.Product.PhotoUrls.FirstOrDefault(),
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.Subtotal
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        // Clears the cart session after successful payment
        public void ClearCart(ISession session)
        {
            session.Clear(); // Removes all session data, including cart
        }

        // Retrieves the current cart from session 
        public List<CartItem> GetCart(ISession session)
        {
            var cart = session.GetString("Cart");
            return string.IsNullOrEmpty(cart) ? new List<CartItem>() : System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart) ?? new List<CartItem>();
        }
    }
}