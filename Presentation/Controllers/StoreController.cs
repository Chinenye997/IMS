using Application.DTOs;
using Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.Extensions;

namespace Presentation.Controllers
{
    [Authorize(Roles ="Admin, Agent, NormalUser")]
    public class StoreController : Controller
    {
        private readonly IProductInterface _productService;
        private readonly ICartInterface _cartService;
        

        public StoreController(IProductInterface productService, ICartInterface cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        // GET: /Store
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var all = (await _productService.GetAllProducts())
                .Where(p => p.IsActive && p.Quantity > 0)
                .ToList();

            // Load each product’s photos
            foreach (var p in all)
                p.PhotoUrls = (await _productService.GetProductById(p.Id)).PhotoUrls;

            // Pass cart to view
            ViewBag.Cart = _cartService.GetCart(HttpContext.Session);

            // Group products by category
            var grouped = all
                .GroupBy(p => p.CategoryName)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(grouped); // Send grouped result to view
        }


        // POST: /Store/AddToCart adding item to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity)
        {
            var updatedCart = await _cartService.AddToCartAsync(
                HttpContext.Session,
                _productService,
                productId,
                quantity
            );

            return PartialView("_CartPopup", updatedCart);
        }

        // GET: /Store/Cart (AJAX cart refresh)
        [HttpGet]
        public IActionResult Cart()
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            return PartialView("_CartPopup", cart);
        }

        // POST: /Store/Checkout - Show payment popup
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            ////Get their current cart
            //var cart = _cartService.GetCart(HttpContext.Session);

            ////For each item in their cart...
            //foreach (var item in cart)
            //{
            //    //Load the product details
            //    var prod = await _productService.GetProductById(item.ProductId);

            //    //If we can’t find the product at all
            //    if (prod == null)
            //    {
            //        TempData["Error"] = $"Product {item.ProductId} no longer exists.";
            //        return RedirectToAction(nameof(Index));  // send them back to store page
            //    }

            //    //If they want more than we have
            //    if (item.Quantity > prod.Quantity)
            //    {
            //        TempData["Error"] = $"Not enough stock for {prod.Name}.";
            //        return RedirectToAction(nameof(Index));  // send them back to store page
            //    }

            //    //Otherwise everything’s OK—record that sale
            //    await _productService.SellProductAsync(item.ProductId, item.Quantity);
            //}

            //// After looping through every cart item, clear it and show success
            //_cartService.ClearCart(HttpContext.Session);
            //TempData["Success"] = "Purchase complete!";
            //return RedirectToAction("Create", "Payment");

            var cart = _cartService.GetCart(HttpContext.Session);
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }
            // Return payment form for popup
            return PartialView("_PaymentForm", new OrderRequest());
        }

        // POST: /Store/UpdateCart - Update cart item quantity
        [HttpPost]
        public async Task<IActionResult> UpdateCart(string productId, string actionType)
        {

            // Load current cart
            var cart = _cartService.GetCart(HttpContext.Session);

            // Find the item
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                // Increase or decrease
                if (actionType == "increase" && item.Quantity < int.MaxValue)
                    item.Quantity++;
                else if (actionType == "decrease" && item.Quantity > 1)
                    item.Quantity--;

                //Save back to session
                _cartService.SaveCart(HttpContext.Session, cart);
            }
              // Return updated partial
             return PartialView("_CartPopup", cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(string productId)
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
            }
            _cartService.SaveCart(HttpContext.Session, cart);
            return PartialView("_CartPopup", cart);
        }


        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart(HttpContext.Session);
            return PartialView("_CartPopup", new List<CartItem>());
        }

    }
}