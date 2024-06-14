using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers;

public class BaseController : Controller
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
}