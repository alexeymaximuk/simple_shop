namespace Shop.Frontend.Models;

public class ProductFilterViewModel
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}