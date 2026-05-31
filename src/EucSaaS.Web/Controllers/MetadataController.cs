using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class MetadataController : Controller
{
    private readonly AppDbContext _db;

    public MetadataController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult EmployeeList()
    {
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetScreenMetadata(string screenCode)
    {
        var roleName = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var screen = await _db.ScreenDefinitions
            .Include(x => x.Columns)
            .Include(x => x.FormFields)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode && x.IsActive);

        if (screen == null)
        {
            return NotFound();
        }

        var permission = screen.Permissions
            .FirstOrDefault(x => x.RoleName == roleName);

        if (permission == null || !permission.CanView)
        {
            return Forbid();
        }

        var result = new
        {
            screen.ScreenCode,
            screen.ScreenName,
            screen.EntityName,
            screen.RoutePath,
            Columns = screen.Columns
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.FieldName,
                    x.DisplayLabel,
                    x.DataType,
                    x.DisplayOrder,
                    x.IsSortable,
                    x.IsSearchable
                }),
            FormFields = screen.FormFields
                .Where(x => x.IsVisible)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.FieldName,
                    x.DisplayLabel,
                    x.ControlType,
                    x.DataType,
                    x.DisplayOrder,
                    x.IsRequired,
                    x.IsReadOnly,
                    x.MaxLength,
                    x.Placeholder
                }),
            Permission = new
            {
                permission.CanView,
                permission.CanAdd,
                permission.CanEdit,
                permission.CanDelete,
                permission.CanExport
            }
        };

        return Json(result);
    }
}
