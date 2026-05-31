using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EucSaaS.Web.Security;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = AppPolicies.ReadAccess)]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
