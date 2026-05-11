using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ProductEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
}