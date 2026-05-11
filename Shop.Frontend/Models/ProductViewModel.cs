namespace Shop.Frontend.Models;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreateTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}