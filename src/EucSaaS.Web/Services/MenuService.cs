using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Security;
using EucSaaS.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services;

public class MenuService
{
    private readonly AppDbContext _db;

    public MenuService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MenuItemViewModel>> GetMenusAsync(Guid tenantId, string roleCode)
    {
        var menus = await _db.AppMenus
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new MenuItemViewModel
            {
                Name = x.Name,
                Url = x.Url,
                Icon = x.Icon,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync();

        return roleCode switch
        {
            AppRoles.Admin => menus,

            AppRoles.Manager => menus
                .Where(x => x.Name is "Dashboard")
                .ToList(),

            AppRoles.User => menus
                .Where(x => x.Name is "Dashboard")
                .ToList(),

            AppRoles.ReadOnly => menus
                .Where(x => x.Name is "Dashboard")
                .ToList(),

            _ => new List<MenuItemViewModel>()
        };
    }
}
