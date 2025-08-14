using Application.DTOs;
using Application.Interface;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin,Agent")]
    public class CategoryController : Controller
    {
        private readonly ICategoryInterface _categoryInterface;
        private readonly AppDbContext _context;

        public CategoryController(ICategoryInterface categoryInterface, AppDbContext context)
        {
            _categoryInterface = categoryInterface;
            _context = context;
        }

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var categories = await _categoryInterface.Get();
                // Add the search logic here
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    categories = categories
                        .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                // KEEP SEARCH TERM IN VIEW
                ViewBag.CurrentFilter = searchTerm;
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var category = await _categoryInterface.GetById(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Index");
                }
                // Add this line to Populate Categories dropdown
                ViewBag.AllCategories = await _context.Categories
                    .Where(c => c.Id != id && c.IsActive) // Exclude current category
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(); // GET: Shows the create form
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CategoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    TempData["Error"] = "Category name is required.";
                    return View(request);
                }
                await _categoryInterface.Create(request);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var category = await _categoryInterface.GetById(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Index");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, UpdateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    TempData["Error"] = "Category name is required.";
                    return View(request);
                }
                request.Id = id.ToString();
                await _categoryInterface.Update(request);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _categoryInterface.Delete(id);
                TempData["Success"] = "Category Deleted Successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> Toggle(string id, bool isActive)
        {
            try
            {
                var updatedCategory = await _categoryInterface.Toggle(id);//IsActive
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        // Reassign product
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReassignProducts(string id,  string newCategoryId)
        {
            try
            {
                // Get products in current category
                var products = await _context.Products
                    .Where(p => p.CategoryId == id)
                    .ToListAsync();

                if(products.Count == 0)
                {
                    TempData["Warning"] = "No product to reassign.";
                    return RedirectToAction("Detail", new { id });
                }

                // Update category for all products
                foreach (var product in products)
                {
                    product.CategoryId = newCategoryId;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Reassigned {products.Count} products successfuly.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error reassign products:{ex.Message}";
                
            }
            return RedirectToAction("Details", new { id });
        }
    }
}