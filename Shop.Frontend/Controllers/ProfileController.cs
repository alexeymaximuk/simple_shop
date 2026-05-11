using Microsoft.AspNetCore.Mvc;

namespace Shop.Frontend.Controllers;

public class ProfileController : Controller
{
    public IActionResult MyProducts()
    {
        return View();
    }
} 