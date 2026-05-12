using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ChangeNameViewModel
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; }
}