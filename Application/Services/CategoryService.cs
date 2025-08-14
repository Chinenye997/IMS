using Application.DTOs;
using Application.Interface;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryInterface
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CategoryResponse>> Get()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    Products = c.Products.Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        CreatedAt = p.CreatedAt,
                        CreatedBy = p.CreatedBy
                    }).ToList()
                })
                .ToListAsync();

            return categories;
        }

        public async Task<CategoryResponse> GetById(string id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.Id == id)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    Products = c.Products.Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        CreatedAt = p.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null) throw new Exception("Category not found");
            return category;
        }

        public async Task<CategoryResponse> Create(CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Category is required");
            }
            if (request.Name.Length > 100)
            {
                throw new ArgumentException("Category name can't exceed 100 character");
            }
            // GET CURRENT USER 
            var username = "System";
            if(_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            }
            
                var category = new CategoryEntity
                {
                    Name = request.Name,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow, // Set current time
                    CreatedBy = username // Placeholder; replace with actual user ID if possible
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    CreatedBy = category.CreatedBy
                };
        }

        public async Task<CategoryResponse> Update(UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(request.Id);
            if (category == null)
            {
                throw new Exception("Category not found");
            }
            category.Name = request.Name;
            await _context.SaveChangesAsync();
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy
            };
        }

        public async Task<bool> Delete(string id)
        {
            var category = await _context.Categories //FindAsync(id)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
             
            if (category == null)
            {
                throw new Exception("Categories not found");
            }
            //CHECK
            if(category.Products != null && category.Products.Any())
            {
                throw new Exception("Cannot delete category with associated products. " +
                                    "Please delete or reassign products first.");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CategoryResponse> Toggle(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new Exception("Category not found");
            }
            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy
            };
        }
    }
}