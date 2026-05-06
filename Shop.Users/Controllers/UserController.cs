using Microsoft.AspNetCore.Mvc;
using Shop.Users.Services.Interfaces;

namespace Shop.Users.Controllers;

[ApiController]
[Route("api/users")]
public partial class UserController (
    IUserService userService,
    IAuthService authService
) : ControllerBase;
