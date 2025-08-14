
using Application.DTOs;

namespace Application.Interface
{
    public interface IProductInterface
    {
        // Get all active products
        Task<List<ProductResponse>> GetAllProducts();
        // Get a product by ID
        Task<ProductResponse> GetProductById(string id);
        // Create a new product
        Task<ProductResponse> CreateProduct(ProductRequest request, string createBy);
        // Update an existing product
        Task<ProductResponse> UpdateProduct(string id, ProductRequest request, List<string> photosToDelete);
        // Get product data for editing
        Task<ProductRequest> GetProductForEdit(string id);
        // Delete a product
        Task<bool> DeleteProduct(string id);
        // Filter and sort products
        Task<List<ProductResponse>> GetProductsFiltered(string categoryId = null, string sortBy = null, bool ascending = true);
        // Search products by name or category
        Task<List<ProductResponse>> SearchProducts(string searchTerm);
        // Calculate total stock value
        Task<decimal> CalculateTotalStockValue();
        // Get top-selling products
        Task<List<MostSoldDto>> GetTopSellingProductsAsync(int topN = 5);
        // Get low-stock products
        Task<List<ProductResponse>> GetLowStockProductsAsync(int threshold = 10);
        // Record a manual sale (returns order details)
        Task<OrderResponse> SellProductAsync(string productId, int quantityToSell, string userId, string paymentMethod);
        Task<CategoryResponse> Toggle(string id, bool isActive);

    }
}
