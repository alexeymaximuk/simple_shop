using Microsoft.AspNetCore.Mvc;
using Shop.Shared.Controllers;

namespace Shop.Frontend.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
