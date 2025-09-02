
using Application.DTOs;
using Application.Interface;
using Azure.Core;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Application.Services;
  // Handles product-related operations
public class ProductService : IProductInterface
{
  private readonly AppDbContext _context;
    private readonly IHostingEnvironment _environment;

    public ProductService(AppDbContext context, IHostingEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }
    // Get all active products
    public async Task<List<ProductResponse>> GetAllProducts()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    // Get a single product by ID
    public async Task<ProductResponse> GetProductById(string id)
    {
        var productEntity = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productEntity == null)
            return null;

        var response = new ProductResponse
        {
            Id = productEntity.Id,
            Name = productEntity.Name,
            Price = productEntity.Price,
            Quantity = productEntity.Quantity,
            Description = productEntity.Description,
            CategoryId = productEntity.CategoryId,
            CategoryName = productEntity.Category?.Name,
            IsActive = productEntity.IsActive,
            CreatedAt = productEntity.CreatedAt,
            PhotoUrls = new List<string>()
        };

        // Populate photo URLs by scanning wwwroot/uploads for files prefixed with product ID
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (Directory.Exists(uploadsFolder))
        {
            var pattern = $"{productEntity.Id}_*";
            var files = Directory.GetFiles(uploadsFolder, pattern);
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                response.PhotoUrls.Add("/uploads/" + fileName);
            }
        }

        return response;
    }

    // Create a new product
    public async Task<ProductResponse> CreateProduct(ProductRequest request, string createdBy)
    {
        var product = new ProductEntity
        {
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Save uploaded photos
        if (request.ProductPhotos?.Any() == true)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in request.ProductPhotos)
            {
                if (file.Length > 0)
                {
                    var uniqueName = $"{product.Id}_{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                    var fullPath = Path.Combine(uploadsFolder, uniqueName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
            }
        }

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId,
            Description = request.Description,
            CategoryName = (await _context.Categories.FindAsync(product.CategoryId))?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            PhotoUrls = new List<string>() // will be populated on GET
        };
    }

    // Update an existing product
    public async Task<ProductResponse> UpdateProduct(string id, ProductRequest request, List<string> photosToDelete)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return null;

        product.Name = request.Name;
        product.Price = request.Price;
        product.Quantity = request.Quantity;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        await _context.SaveChangesAsync();

        // Delete selected photos
        if (photosToDelete != null && photosToDelete.Any())
        {
            foreach (var url in photosToDelete)
            {
                var fileName = Path.GetFileName(url);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        // Save new uploaded photos
        if (request.ProductPhotos?.Any() == true)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in request.ProductPhotos)
            {
                if (file.Length > 0)
                {
                    var uniqueName = $"{product.Id}_{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                    var fullPath = Path.Combine(uploadsFolder, uniqueName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
            }
        }

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = (await _context.Categories.FindAsync(product.CategoryId))?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            PhotoUrls = new List<string>()
        };
    }

    // Delete a product
    public async Task<bool> DeleteProduct(string id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            var orderItems = await _context.OrderItems.Where(oi => oi.ProductId == id).ToListAsync();
            _context.OrderItems.RemoveRange(orderItems);
            _context.Remove(product);
           await _context.SaveChangesAsync();
            return true;                            
        }
        return false;
    }

    // Filter and sort products
    public async Task<List<ProductResponse>> GetProductsFiltered(string categoryId = null, string sortBy = null, bool ascending = true)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    query = ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    query = ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "quantity":
                    query = ascending ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity);
                    break;
            }
        }

        return await query.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Quantity = p.Quantity,
            Description =p. Description,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        }).ToListAsync();
    }

    // Search products by name or category
    public async Task<List<ProductResponse>> SearchProducts(string searchTerm)
    {
        // Convert search term to lowercase for case-insensitive search
        searchTerm = searchTerm.ToLower() ?? string.Empty;

        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(searchTerm) || p.Category.Name.ToLower().Contains(searchTerm))
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToListAsync();
        // Searches product name and category name, returns matching products
    }

    // Calculate total stock value
    public async Task<decimal> CalculateTotalStockValue()
    {
        // sum (price * quantity) across all products
        return await _context.Products
             .SumAsync(p => p.Price * p.Quantity);
    }

    // Get top-selling products based on order items
    public async Task<List<MostSoldDto>> GetTopSellingProductsAsync(int topN = 5)
    {
        // Group order items by product, sum quantities, take top N
        var query = _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(topN);

        // Join with Products to get the Name
        //Join with product info
        var result = await query
            .Join(_context.Products,
                  sold => sold.ProductId,
                  prod => prod.Id,
                  (sold, prod) => new MostSoldDto
                  {
                      ProductId = prod.Id,
                      Name = prod.Name,
                      TotalSold = sold.TotalSold
                  })
            .ToListAsync();

        return result;
    }

    // Record a manual sale (e.g., in-store by Admin/Agent)
    public async Task<OrderResponse> SellProductAsync(string productId, int quantityToSell, string userId, string paymentMethod)
    {
        // Check if product exists and has enough stock
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found.");
        }
        if (quantityToSell <= 0 || quantityToSell > product.Quantity)
        {
            throw new ArgumentException("Invalid quantity to sell.");
        }

        // Start transaction to keep changes together
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Generate InvoiceNo
            var orderCount = await _context.Orders.CountAsync() + 1;
            var invoiceNo = $"InvoiceNo-{orderCount:D3}"; // Pads with zeros (e.g., 001)

            // Create order
            var order = new Order
            {
                UserId = userId,
                TotalAmount = product.Price * quantityToSell,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Completed", // Cash/Transfer are instant
                PaidAt = DateTime.UtcNow,
                InvoiceNo = invoiceNo
            };
            _context.Orders.Add(order);

            // Create order item
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = productId,
                Quantity = quantityToSell,
                UnitPrice = product.Price,
                Subtotal = product.Price * quantityToSell
            };
            _context.OrderItems.Add(orderItem);

            // Update product stock
            product.Quantity -= quantityToSell;
            _context.Products.Update(product);

            // Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return order details
            return new OrderResponse
            {
                Id = order.Id,
                InvoiceNo = order.InvoiceNo,
                CustomerName = (await _context.Users.FindAsync(userId))?.FullName ?? "Unknown",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                PaidAt = order.PaidAt,
                Items = new List<OrderItemResponse>
                    {
                        new OrderItemResponse
                        {
                            ProductId = productId,
                            ProductName = product.Name,
                            PhotoUrl = product.PhotoUrls.FirstOrDefault() ?? "", // Use first photo or empty string
                            Quantity = quantityToSell,
                            UnitPrice = product.Price,
                            Subtotal = product.Price * quantityToSell
                        }
                    }
            };
        }
        catch
        {
            // Undo changes if something fails
            await transaction.RollbackAsync();
            throw;
        }
    }





    public async Task<ProductRequest> GetProductForEdit(string id)
    {
        // Reuse the existing GetProductById to grab the response DTO
        var product = await GetProductById(id);
        if(product == null)
        {
            return null;
        }
        // Map into the request/edit model
        var request = new ProductRequest
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            Description = product.Description,
            CategoryId = product.CategoryId,
            PhotoUrls = product.PhotoUrls,
            Categories = (await _context.Categories
            .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name })
            .ToListAsync())

        };
        return request;
    }

    public async Task<List<ProductResponse>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await _context.Products
        .Include(p => p.Category)
        .Where(p => p.Quantity < threshold && p.IsActive)
        .Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Quantity = p.Quantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        }).ToListAsync();
        if (!products.Any())
            Console.WriteLine("No low stock products found with threshold " + threshold);
        return products;
    }

    public async Task<CategoryResponse> Toggle(string id, bool isActive)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new Exception("Category not found");
        }
        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();
        return new CategoryResponse
        {
            Id = product.Id,
            Name = product.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            CreatedBy = product.CreatedBy
        };
    }

    public async Task<bool> UpdateProductQuantityAsync(int productId, int newQuantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if(product == null)
        {
            return false;
        }
        product.Quantity = newQuantity;
        await _context.SaveChangesAsync();
        return true;
        //throw new NotImplementedException();
    }
}
