using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize]
[Route("api/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _db.Employees
            .OrderBy(x => x.EmployeeCode)
            .Select(x => new
            {
                id = x.Id,
                employeeCode = x.EmployeeCode,
                fullName = x.FullName,
                department = x.Department,
                jobTitle = x.JobTitle,
                email = x.Email,
                status = x.Status,
                createdDate = x.CreatedDate.ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeRequest request)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            Department = request.Department,
            JobTitle = request.JobTitle,
            Email = request.Email,
            Status = request.Status,
            CreatedDate = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return Ok(new { id = employee.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeRequest request)
    {
        var employee = await _db.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        employee.EmployeeCode = request.EmployeeCode;
        employee.FullName = request.FullName;
        employee.Department = request.Department;
        employee.JobTitle = request.JobTitle;
        employee.Email = request.Email;
        employee.Status = request.Status;

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        return Ok();
    }
}

public class EmployeeRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
