namespace EucSaaS.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class Employee
{
    public Guid Id { get; set; }

[Required]
public string EmployeeCode { get; set; } = string.Empty;

[Required]
public string FullName { get; set; } = string.Empty;

[Required]
[EmailAddress]
public string Email { get; set; } = string.Empty;

[Required]
public string Department { get; set; } = string.Empty;

public string? JobTitle { get; set; }

[Required]
public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
