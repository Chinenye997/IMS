
namespace Application.DTOs;

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryRequest : CategoryRequest
{
    public string Id { get; set; } = string.Empty;
}

public class CategoryResponse : UpdateCategoryRequest
{
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public List<ProductResponse> Products { get; set; } = new List<ProductResponse>(); // Add this
}