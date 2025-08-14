using Application.DTOs;
using Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinancesController : Controller
    {
        private readonly IProductInterface _products;
        public FinancesController(IProductInterface products) => _products = products;

        // GET: /Finances
        public async Task<IActionResult> Index()
        {
            // 1. Total stock value
            var totalValue = await _products.CalculateTotalStockValue();

            // 2. Top 5 best-selling products
            var topSellers = await _products.GetTopSellingProductsAsync(5);

            // Combine into a simple ViewModel
            var vm = new Finances
            {
                TotalStockValue = totalValue,
                TopSellers = topSellers
            };
            return View(vm);
        }
    }
}
