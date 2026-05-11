using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}