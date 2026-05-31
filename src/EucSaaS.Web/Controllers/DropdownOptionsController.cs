using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DropdownOptionsController : Controller
{
    private readonly AppDbContext _context;

    public DropdownOptionsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var options = await _context.FormFieldOptionDefinitions
            .Include(x => x.FormFieldDefinition)
            .OrderBy(x => x.FormFieldDefinition.FieldName)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        return View(options);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Fields =
            await _context.FormFieldDefinitions
                .OrderBy(x => x.FieldName)
                .ToListAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        FormFieldOptionDefinition option)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Fields =
                await _context.FormFieldDefinitions
                    .OrderBy(x => x.FieldName)
                    .ToListAsync();

            return View(option);
        }

        option.Id = Guid.NewGuid();

        _context.FormFieldOptionDefinitions
            .Add(option);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Option created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var option =
            await _context.FormFieldOptionDefinitions
                .FirstOrDefaultAsync(x => x.Id == id);

        if (option == null)
            return NotFound();

        ViewBag.Fields =
            await _context.FormFieldDefinitions
                .OrderBy(x => x.FieldName)
                .ToListAsync();

        return View(option);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        Guid id,
        FormFieldOptionDefinition model)
    {
        var option =
            await _context.FormFieldOptionDefinitions
                .FirstOrDefaultAsync(x => x.Id == id);

        if (option == null)
            return NotFound();

        option.FormFieldDefinitionId =
            model.FormFieldDefinitionId;

        option.OptionLabel =
            model.OptionLabel;

        option.OptionValue =
            model.OptionValue;

        option.DisplayOrder =
            model.DisplayOrder;

        option.IsActive =
            model.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Option updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var option =
            await _context.FormFieldOptionDefinitions
                .FirstOrDefaultAsync(x => x.Id == id);

        if (option == null)
            return NotFound();

        _context.FormFieldOptionDefinitions
            .Remove(option);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Option deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
