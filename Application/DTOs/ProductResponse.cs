
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ProductResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price {  get; set; }
        public int Quantity {  get; set; }
        public string? Description { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();

    }

    public class ProductRequest
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage ="Product name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price {  get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage ="Category is required.")]
        public string CategoryId { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<IFormFile> ProductPhotos { get; set; } = new List<IFormFile>();
        public List<string> PhotoUrls { get; set; } = new();
        public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();

    }
}
