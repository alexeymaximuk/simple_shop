namespace Shop.Products.DTOs;

public class ProductFilterDto
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? UserId { get; set; }
}