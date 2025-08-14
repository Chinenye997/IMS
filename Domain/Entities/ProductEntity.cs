using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductEntity:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        [Column(TypeName ="Decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public string CategoryId { get; set; } = string.Empty; //Link to category
        public CategoryEntity ? Category { get; set; } = null; // navigation property
        public List<string> PhotoUrls { get; set; } = new List<string>();


    }
}
