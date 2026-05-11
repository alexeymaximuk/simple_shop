using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ResendConfirmationViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}