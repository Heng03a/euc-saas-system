using System.Security.Claims;
using EucSaaS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EucSaaS.Web.ViewComponents;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly MenuService _menuService;

    public SidebarMenuViewComponent(MenuService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var tenantIdValue = UserClaimsPrincipal.FindFirst("TenantId")?.Value;
        var roleCode = UserClaimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            return View(new List<ViewModels.MenuItemViewModel>());
        }

        var menus = await _menuService.GetMenusAsync(tenantId, roleCode);

        return View(menus);
    }
}
