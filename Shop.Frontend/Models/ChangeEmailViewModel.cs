using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ChangeEmailViewModel
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; }
}