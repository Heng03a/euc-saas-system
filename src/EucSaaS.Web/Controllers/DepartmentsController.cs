using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Security;
using EucSaaS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = AppPolicies.ManagerOrAdmin)]
public class DepartmentsController : Controller
{
    private readonly AppDbContext _db;

    public DepartmentsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _db.Departments
            .OrderBy(x => x.Code)
            .Select(x => new DepartmentListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            })
            .ToListAsync();

        return View(departments);
    }
}
