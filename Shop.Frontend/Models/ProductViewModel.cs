namespace Shop.Frontend.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int UserId { get; set; }
    public DateTime CreateDate { get; set; }
}