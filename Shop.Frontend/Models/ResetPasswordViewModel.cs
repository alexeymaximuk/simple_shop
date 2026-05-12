using System.ComponentModel.DataAnnotations;

namespace Shop.Frontend.Models;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; }
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}