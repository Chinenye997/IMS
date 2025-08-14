
namespace Domain;

public class BaseEntity
{
    public BaseEntity()
    {
        Id = Guid.NewGuid().ToString().Replace("-", "");
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public string Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
