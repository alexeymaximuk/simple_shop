namespace Shop.Products.Application.DTOs;

public class ProductResponseDto
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