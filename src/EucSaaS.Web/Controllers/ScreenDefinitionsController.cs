using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class ScreenDefinitionsController : Controller
{
    private readonly AppDbContext _context;

    public ScreenDefinitionsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var screens = await _context.ScreenDefinitions
            .OrderBy(x => x.ScreenCode)
            .ToListAsync();

        return View(screens);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.Columns)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (screen == null)
        {
            return NotFound();
        }

        screen.Columns = screen.Columns
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        return View(screen);
    }
}
