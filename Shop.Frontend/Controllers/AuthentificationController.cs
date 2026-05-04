using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shop.Frontend.Models;

namespace Shop.Frontend.Controllers;

public class AuthentificationController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}