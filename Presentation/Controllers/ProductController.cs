using Application.DTOs;
using Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;


namespace Presentation.Controllers
{
    // Allow all (or authenticated) to see list/details
    //[AllowAnonymous]
    [Authorize(Roles = "Admin,Agent")]
    public class ProductController : Controller
    {
        private readonly IProductInterface _productService;
        private readonly ICategoryInterface _categoryInterface;
     
        public ProductController(IProductInterface productService, ICategoryInterface categoryInterface)
        {
            _productService = productService;
            _categoryInterface = categoryInterface;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string categoryId, string sortBy, bool ascending = true, string searchTerm = null)
        {
            // Handles filtering, sorting, and searching
            var products = string.IsNullOrEmpty(searchTerm)
                ? await _productService.GetProductsFiltered(categoryId, sortBy, ascending)
                : await _productService.SearchProducts(searchTerm);   //Populates category dropdown
            ViewBag.TotalValue = await _productService.CalculateTotalStockValue();
            ViewBag.Categories = (await _categoryInterface.Get()).ToList();  //Passes search term to view for display
            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }

        [Authorize] // All authenticated users can view
        public async Task<IActionResult> Details(string id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Admin & Agent can create/edit/restock/sell
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Create(string categoryId)
        {
            var productRequest = new ProductRequest
            {
                Categories = (await _categoryInterface.Get()).ToList()
            };
            return View(productRequest);

        }
        [HttpPost]
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> Create(ProductRequest request)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateProduct(request, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous");
                TempData["Success"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            request.Categories = (await _categoryInterface.Get()).ToList();
            return View(request);
        }

        

        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> Edit(string id)
        {   
            var model = await _productService.GetProductForEdit(id);
            if (model == null) return NotFound();

            // Now CategoryList is a SelectList of (Id, Name), with the current CategoryId selected
            ViewBag.CategoryList = new SelectList(
                model.Categories,    // the list of options
                "Id",                // value field
                "Name",              // text field
                model.CategoryId     // the item to be selected by default
            );

            return View(model);
        }

        

        [HttpPost]
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> Edit(string id, ProductRequest request, List<string> photosToDelete)
        {
            if (ModelState.IsValid)
            {
                await _productService.UpdateProduct(id, request, photosToDelete);
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            request.Categories = (await _categoryInterface.Get()).ToList();
            return View(request);


        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _productService.DeleteProduct(id);
            TempData["Success"] = "Product Deleted!";
            return RedirectToAction(nameof(Index));
        }

        // Restock - Show restock form
        [HttpGet]
        [Authorize(Roles ="Admin, Agent")]
        public async Task<IActionResult> Restock(string id)
        {
            var product = await _productService.GetProductById(id);
            if(product == null)
            {
                return NotFound();
            }
            ViewBag.productName = product.Name;
            ViewBag.productId = product.Id;
            return View(); // This will use Views/Product/Restock.cshtml
        }

        // update quantity:
        [HttpPost]
        [Authorize(Roles ="Admin, Agent")]
        public async Task<IActionResult> Restock(string id, int quantityToAdd, List<string> photosToDelete)
        {
            // Manually increase quantity
            var product = await _productService.GetProductById(id);
            if(product == null)
            {
                return NotFound();
            }
            var updateRequest = new ProductRequest
            {
                Name = product.Name,                
                Price = product.Price,
                Quantity = product.Quantity + quantityToAdd,
                CategoryId = product.CategoryId
            };
            await _productService.UpdateProduct(id, updateRequest, photosToDelete);
            TempData["Success"] = $"Add {quantityToAdd} new stock!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Product/Sell - Show sell form
        [HttpGet]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Sell(string id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();
            ViewBag.ProductId = product.Id;
            ViewBag.ProductName = product.Name;
            ViewBag.CurrentQuantity = product.Quantity;
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cash", Text = "Cash" },
                new SelectListItem { Value = "Transfer", Text = "Bank Transfer" }
            };
            return View();
        }

        // POST: /Product/Sell - Record manual sale
        [HttpPost]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Sell(string id, int quantityToSell, string paymentMethod)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                var order = await _productService.SellProductAsync(id, quantityToSell, userId, paymentMethod);
                TempData["Success"] = $"Sold {quantityToSell} units of {order.Items.First().ProductName}!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                var product = await _productService.GetProductById(id);
                ViewBag.ProductId = id;
                ViewBag.ProductName = product?.Name;
                ViewBag.CurrentQuantity = product?.Quantity ?? 0;
                ViewBag.PaymentMethods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Cash", Text = "Cash" },
                    new SelectListItem { Value = "Transfer", Text = "Bank Transfer" }
                };
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.PaymentMethods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Cash", Text = "Cash" },
                    new SelectListItem { Value = "Transfer", Text = "Bank Transfer" }
                };
                return View();
            }
        }

        //StockValueReport
        [Authorize(Roles ="Admin, Agent")]
        public async Task<IActionResult> StockValueReport()
        {
            // Call service to calculate the total value
            var totalValue = await _productService.CalculateTotalStockValue();
            //pass it to the view
            ViewBag.TotalValue = totalValue;
            return View();
        }

        // for TopSellers
        [Authorize(Roles ="Admin, Agent")]
        public async Task<IActionResult> TopSellers()
        {
            var topSellers = await _productService.GetTopSellingProductsAsync();
            return View(topSellers);
        }

        // GET: /Product/LowStock - Show low-stock products
        [Authorize(Roles = "Admin,Agent")]
        [HttpGet]
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            // Fetch all products with low stock
            var lowStockProducts = await _productService.GetLowStockProductsAsync(threshold);
            ViewBag.Threshold = threshold;
            return View(lowStockProducts);
        }

        [Authorize(Roles = "Admin,Agent")]
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int productId, int newQuantity)
        {
            if (productId <= 0 || newQuantity < 0)
            {
                TempData["Error"] = "Invalid product ID or quantity.";
                return RedirectToAction("LowStock");
            }
            Console.WriteLine($"Updating product {productId} to quantity {newQuantity}");
            var result = await _productService.UpdateProductQuantityAsync(productId, newQuantity);
            if (result)
            {
                TempData["Success"] = "Stock updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update stock.";
            }
            return RedirectToAction("LowStock");
        }

        public async Task<IActionResult> Toggle(string id, bool isActive)
        {
            //var product = await _productService.GetProductById(id);
            //if (product == null) return NotFound();
            //product.IsActive = isActive;
            //await _productService.UpdateProduct(product);
            //return RedirectToAction("Index");
            try
            {
                var updatedProduct = await _productService.Toggle(id, isActive);//IsActive
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}