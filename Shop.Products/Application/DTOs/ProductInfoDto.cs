namespace Shop.Products.Application.DTOs;

public class ProductInfoDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
}