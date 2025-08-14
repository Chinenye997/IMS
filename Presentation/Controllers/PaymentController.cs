using Application.DTOs;
using Application.Interface;
using Application.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Presentation.Controllers
{

    // Restrict to authenticated users
    [Authorize(Roles = "Admin,Agent,NormalUser")]
    public class PaymentController : Controller
    {
        private readonly IPaymentInterface _paymentService;
        private readonly ICartInterface _cartService;

        public PaymentController(IPaymentInterface paymentService, ICartInterface cartService)
        {
            _paymentService = paymentService;
            _cartService = cartService;
        }

        // POST: /Payment/Process - Handle payment form submission
        [HttpPost]
        public async Task<IActionResult> Process(OrderRequest request)
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            if (!cart.Any())
                return Json(new { success = false, message = "Cart is empty." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var success = await _paymentService.ProcessOrderAsync(userId, request, cart);

            if (!success)
                return Json(new { success = false, message = "Payment failed. Check stock or try again." });

            // Clear cart and redirect to store
            _cartService.ClearCart(HttpContext.Session);
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Store") });
        }

        // GET: /Payment/Index - Show order history
        [HttpGet]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Index(string search)
        {
            var orders = await _paymentService.GetAllOrdersAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                orders = orders
                    .Where(o =>
                        !string.IsNullOrEmpty(o.InvoiceNo) && o.InvoiceNo.ToLower().Contains(search) || // Safe InvoiceNo search
                        !string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(search) || // Safe CustomerName search
                        !string.IsNullOrEmpty(o.PaymentMethod) && o.PaymentMethod.ToLower().Contains(search) ||
                        !string.IsNullOrEmpty(o.PaymentStatus) && o.PaymentStatus.ToLower().Contains(search) ||
                        o.OrderDate.ToString("g").ToLower().Contains(search)
                    )
                    .ToList();
            }

            ViewBag.Search = search;
            return View(orders);
        }

        // GET: /Payment/Details - Show order details
        [HttpGet]
        public async Task<IActionResult> Details(string invoiceNo)
        {
            if (string.IsNullOrEmpty(invoiceNo))
                return NotFound();

            var order = await _paymentService.GetOrderDetailsAsync(invoiceNo);
            if (order == null)
                return NotFound();

            ViewBag.OrderId = invoiceNo; // Use InvoiceNo for display
            return View(order);
        }
    }
}
    
