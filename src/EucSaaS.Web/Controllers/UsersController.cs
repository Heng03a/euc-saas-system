using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Security;
using EucSaaS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(
    string? search,
    string searchColumn = "Username",
    string searchOperator = "Contains",
    string sortBy = "Username",
    string sortDir = "asc",
    int page = 1,
    int pageSize = 5)

    {
        if (page < 1)
        {
            page = 1;
        }

        var query = _db.AppUsers
            .Include(x => x.Role)
            .Include(x => x.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
{
    query = (searchColumn, searchOperator) switch
    {
        ("Username", "Contains") => query.Where(x => x.Username.Contains(search)),
        ("Username", "Equals") => query.Where(x => x.Username == search),
        ("Username", "StartsWith") => query.Where(x => x.Username.StartsWith(search)),
        ("Username", ">") => query.Where(x => string.Compare(x.Username, search) > 0),
        ("Username", "<") => query.Where(x => string.Compare(x.Username, search) < 0),

        ("FullName", "Contains") => query.Where(x => x.FullName.Contains(search)),
        ("FullName", "Equals") => query.Where(x => x.FullName == search),
        ("FullName", "StartsWith") => query.Where(x => x.FullName.StartsWith(search)),
        ("FullName", ">") => query.Where(x => string.Compare(x.FullName, search) > 0),
        ("FullName", "<") => query.Where(x => string.Compare(x.FullName, search) < 0),

        ("Email", "Contains") => query.Where(x => x.Email.Contains(search)),
        ("Email", "Equals") => query.Where(x => x.Email == search),
        ("Email", "StartsWith") => query.Where(x => x.Email.StartsWith(search)),

        ("Role", "Contains") => query.Where(x => x.Role != null && x.Role.Name.Contains(search)),
        ("Role", "Equals") => query.Where(x => x.Role != null && x.Role.Name == search),

        ("Department", "Contains") => query.Where(x => x.Department != null && x.Department.Name.Contains(search)),
        ("Department", "Equals") => query.Where(x => x.Department != null && x.Department.Name == search),

        _ => query.Where(x =>
            x.Username.Contains(search) ||
            x.FullName.Contains(search))
    };
}

        query = (sortBy, sortDir.ToLower()) switch
        {
            ("FullName", "asc") => query.OrderBy(x => x.FullName),
            ("FullName", "desc") => query.OrderByDescending(x => x.FullName),

            ("Email", "asc") => query.OrderBy(x => x.Email),
            ("Email", "desc") => query.OrderByDescending(x => x.Email),

            ("Role", "asc") => query.OrderBy(x => x.Role!.Name),
            ("Role", "desc") => query.OrderByDescending(x => x.Role!.Name),

            ("Department", "asc") => query.OrderBy(x => x.Department!.Name),
            ("Department", "desc") => query.OrderByDescending(x => x.Department!.Name),

            ("Username", "desc") => query.OrderByDescending(x => x.Username),
            _ => query.OrderBy(x => x.Username)
        };

        var totalRecords = await query.CountAsync();

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItemViewModel
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role != null ? x.Role.Name : "",
                Department = x.Department != null ? x.Department.Name : ""
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDir = sortDir;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalRecords = totalRecords;
        ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        ViewBag.SearchColumn = searchColumn;
        ViewBag.SearchOperator = searchOperator;

        return View(users);
    }
}
