using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EucSaaS.Web.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [Route("admin/{screenCode}")]
    public async Task<IActionResult> Index(
        string screenCode,
        string? search,
        string? filterField,
        string? filterOperator,
        string? filterValue,
        string? sortBy,
        string? sortDir,
        int page = 1,
        int pageSize = 10)
    {
        screenCode = screenCode.ToUpper();

        var screen = await _context.ScreenDefinitions
.Include(x => x.Columns
    .Where(c => c.IsVisible)
    .OrderBy(c => c.DisplayOrder))
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return NotFound();

        IQueryable<Employee> query = _context.Employees;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.ToLower();

            query = query.Where(x =>
                (x.FullName ?? "").ToLower().Contains(searchValue) ||
                (x.Email ?? "").ToLower().Contains(searchValue) ||
                (x.EmployeeCode ?? "").ToLower().Contains(searchValue));
        }

        if (!string.IsNullOrWhiteSpace(filterField) &&
            !string.IsNullOrWhiteSpace(filterOperator) &&
            !string.IsNullOrWhiteSpace(filterValue))
        {
            var safeFields = screen.Columns.Select(x => x.FieldName).ToList();

            if (safeFields.Contains(filterField))
            {
                var value = filterValue.ToLower();

                switch (filterOperator)
                {
                    case "contains":
                        query = query.Where($"{filterField}.ToLower().Contains(@0)", value);
                        break;

                    case "equals":
                        query = query.Where($"{filterField}.ToLower() == @0", value);
                        break;

                    case "startsWith":
                        query = query.Where($"{filterField}.ToLower().StartsWith(@0)", value);
                        break;

                    case "endsWith":
                        query = query.Where($"{filterField}.ToLower().EndsWith(@0)", value);
                        break;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var safeFields = screen.Columns.Select(x => x.FieldName).ToList();

            if (safeFields.Contains(sortBy))
            {
                var direction = sortDir == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{sortBy} {direction}");
            }
        }
        else
        {
            query = query.OrderBy(x => x.FullName);
        }

        var totalRecords = await query.CountAsync();

        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = new List<Dictionary<string, object?>>();

        foreach (var employee in employees)
        {
            var row = new Dictionary<string, object?>
            {
                ["__Id"] = employee.Id
            };

            foreach (var column in screen.Columns)
            {
                var property = employee.GetType().GetProperty(
                    column.FieldName,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                row[column.FieldName] = property?.GetValue(employee);
            }

            rows.Add(row);
        }

        var vm = new AdminListViewModel
        {
            ScreenCode = screen.ScreenCode,
            ScreenName = screen.ScreenName,
            Columns = screen.Columns.Select(x => x.FieldName).ToList(),
            Rows = rows,
            Search = search,
            FilterField = filterField,
            FilterOperator = filterOperator,
            FilterValue = filterValue,
            SortBy = sortBy,
            SortDir = sortDir,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };

        return View(vm);
    }

    [HttpGet("admin/{screenCode}/details/{id}")]
    public async Task<IActionResult> Details(string screenCode, Guid id)
    {
        screenCode = screenCode.ToUpper();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return Redirect($"/admin/{screenCode}");
        }

        ViewBag.ScreenCode = screenCode;
        await LoadDynamicFormMetadataAsync(screenCode);

        return View(employee);
    }

    [HttpGet("admin/{screenCode}/create")]
    public async Task<IActionResult> Create(string screenCode)
    {
        screenCode = screenCode.ToUpper();

        ViewBag.ScreenCode = screenCode;
        await LoadDynamicFormMetadataAsync(screenCode);

        return View(new Employee());
    }

    [HttpPost("admin/{screenCode}/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string screenCode, Employee employee)
    {
        screenCode = screenCode.ToUpper();

        ViewBag.ScreenCode = screenCode;

        if (!ModelState.IsValid)
        {
            await LoadDynamicFormMetadataAsync(screenCode);
            return View(employee);
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Record created successfully.";

        return RedirectToAction(nameof(Index), new { screenCode });
    }

    [HttpGet("admin/{screenCode}/edit/{id}")]
    public async Task<IActionResult> Edit(string screenCode, Guid id)
    {
        screenCode = screenCode.ToUpper();

        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

        if (employee == null)
            return NotFound();

        ViewBag.ScreenCode = screenCode;
        await LoadDynamicFormMetadataAsync(screenCode);

        return View(employee);
    }

    [HttpPost("admin/{screenCode}/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string screenCode, Guid id, Employee employee)
    {
        screenCode = screenCode.ToUpper();

        ViewBag.ScreenCode = screenCode;

        if (id != employee.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await LoadDynamicFormMetadataAsync(screenCode);
            return View(employee);
        }

        var existingEmployee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

        if (existingEmployee == null)
            return NotFound();

        existingEmployee.EmployeeCode = employee.EmployeeCode;
        existingEmployee.FullName = employee.FullName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Department = employee.Department;
        existingEmployee.JobTitle = employee.JobTitle;
        existingEmployee.Status = employee.Status;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Record updated successfully.";

        return RedirectToAction(nameof(Index), new { screenCode });
    }

    [HttpGet("admin/{screenCode}/delete/{id}")]
    public async Task<IActionResult> Delete(string screenCode, Guid id)
    {
        screenCode = screenCode.ToUpper();

        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

        if (employee == null)
            return NotFound();

        ViewBag.ScreenCode = screenCode;

        return View(employee);
    }

    [HttpPost("admin/{screenCode}/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string screenCode, Guid id)
    {
        screenCode = screenCode.ToUpper();

        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Record deleted successfully.";

        return RedirectToAction(nameof(Index), new { screenCode });
    }

    private async Task LoadDynamicFormMetadataAsync(string screenCode)
    {
        var screen = await _context.ScreenDefinitions
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode.ToUpper());

        if (screen == null)
        {
            ViewBag.FormFields = new List<FormFieldDefinition>();
            ViewBag.FieldOptions = new Dictionary<string, List<SelectListItem>>();
            return;
        }

        var formFields = await _context.FormFieldDefinitions
            .Where(x => x.ScreenDefinitionId == screen.Id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var fieldIds = formFields.Select(x => x.Id).ToList();

        var optionRecords = await _context.FormFieldOptionDefinitions
            .Where(x => fieldIds.Contains(x.FormFieldDefinitionId) && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var fieldOptions = formFields.ToDictionary(
            field => field.FieldName,
            field => optionRecords
                .Where(option => option.FormFieldDefinitionId == field.Id)
                .Select(option => new SelectListItem
                {
                    Text = option.OptionLabel,
                    Value = option.OptionValue
                })
                .ToList()
        );

        ViewBag.FormFields = formFields;
        ViewBag.FieldOptions = fieldOptions;
    }
}
