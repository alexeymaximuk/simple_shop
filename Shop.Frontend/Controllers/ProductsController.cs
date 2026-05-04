using Microsoft.AspNetCore.Mvc;

namespace Shop.Frontend.Controllers;

public class ProductsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}