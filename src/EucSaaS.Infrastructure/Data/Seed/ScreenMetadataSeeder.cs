using EucSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Infrastructure.Data.Seed;

public static class ScreenMetadataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.ScreenDefinitions.AnyAsync(x => x.ScreenCode == "EMPLOYEES"))
            return;

        var screen = new ScreenDefinition
        {
            Id = Guid.NewGuid(),
            ScreenCode = "EMPLOYEES",
            ScreenName = "Employees",
            EntityName = "Employee",
            RoutePath = "/Metadata/EmployeeList",
            Description = "Employee maintenance screen",
            IsActive = true
        };

        context.ScreenDefinitions.Add(screen);
        await context.SaveChangesAsync();

        var columns = new List<ColumnDefinition>
        {
            CreateColumn(screen.Id, "employeeCode", "Employee Code", "text", 1, true),
            CreateColumn(screen.Id, "fullName", "Full Name", "text", 2, true),
            CreateColumn(screen.Id, "department", "Department", "text", 3, true),
            CreateColumn(screen.Id, "email", "Email", "email", 4, true),
            CreateColumn(screen.Id, "status", "Status", "text", 5, false)
        };

        var employeeCodeField = CreateField(screen.Id, "employeeCode", "Employee Code", "textbox", "text", 1, true, 50, "Enter employee code");
        var fullNameField = CreateField(screen.Id, "fullName", "Full Name", "textbox", "text", 2, true, 150, "Enter full name");
        var departmentField = CreateField(screen.Id, "department", "Department", "dropdown", "text", 3, false, 100, "Select department");
        var emailField = CreateField(screen.Id, "email", "Email", "textbox", "email", 4, false, 150, "Enter email address");
        var statusField = CreateField(screen.Id, "status", "Status", "dropdown", "text", 5, false, null, "Select status");

        var fields = new List<FormFieldDefinition>
        {
            employeeCodeField,
            fullNameField,
            departmentField,
            emailField,
            statusField
        };

        context.ColumnDefinitions.AddRange(columns);
        context.FormFieldDefinitions.AddRange(fields);

        context.FormFieldOptionDefinitions.AddRange(
            CreateOption(departmentField.Id, "IT", "IT", 1),
            CreateOption(departmentField.Id, "HR", "HR", 2),
            CreateOption(departmentField.Id, "Finance", "Finance", 3),
            CreateOption(departmentField.Id, "Operations", "Operations", 4),
            CreateOption(departmentField.Id, "Sales", "Sales", 5),

            CreateOption(statusField.Id, "Active", "Active", 1),
            CreateOption(statusField.Id, "Inactive", "Inactive", 2)
        );

        await context.SaveChangesAsync();
    }

    private static ColumnDefinition CreateColumn(
        Guid screenId,
        string fieldName,
        string displayLabel,
        string dataType,
        int displayOrder,
        bool isSearchable)
    {
        return new ColumnDefinition
        {
            Id = Guid.NewGuid(),
            ScreenDefinitionId = screenId,
            FieldName = fieldName,
            DisplayLabel = displayLabel,
            DataType = dataType,
            DisplayOrder = displayOrder,
            IsVisible = true,
            IsSortable = true,
            IsSearchable = isSearchable
        };
    }

    private static FormFieldDefinition CreateField(
        Guid screenId,
        string fieldName,
        string displayLabel,
        string controlType,
        string dataType,
        int displayOrder,
        bool isRequired,
        int? maxLength,
        string? placeholder)
    {
        return new FormFieldDefinition
        {
            Id = Guid.NewGuid(),
            ScreenDefinitionId = screenId,
            FieldName = fieldName,
            DisplayLabel = displayLabel,
            ControlType = controlType,
            DataType = dataType,
            DisplayOrder = displayOrder,
            IsRequired = isRequired,
            IsReadOnly = false,
            IsVisible = true,
            MaxLength = maxLength,
            Placeholder = placeholder
        };
    }

    private static FormFieldOptionDefinition CreateOption(
        Guid formFieldDefinitionId,
        string optionLabel,
        string optionValue,
        int displayOrder)
    {
        return new FormFieldOptionDefinition
        {
            Id = Guid.NewGuid(),
            FormFieldDefinitionId = formFieldDefinitionId,
            OptionLabel = optionLabel,
            OptionValue = optionValue,
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }
}
