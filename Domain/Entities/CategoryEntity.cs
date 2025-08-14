
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class CategoryEntity : BaseEntity
{
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; }

    // Add navigation property for products
    public List<ProductEntity> Products { get; set; } = new List<ProductEntity>(); // Allows multiple products
}
