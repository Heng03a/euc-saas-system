using EucSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Infrastructure.Data;

public static class MetadataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ScreenDefinitions.AnyAsync(x => x.ScreenCode == "EMPLOYEE_LIST"))
        {
            return;
        }

        var screen = new ScreenDefinition
        {
            Id = Guid.NewGuid(),
            ScreenCode = "EMPLOYEE_LIST",
            ScreenName = "Employee List",
            EntityName = "Employee",
            RoutePath = "/Metadata/EmployeeList",
            Description = "Metadata-driven employee listing screen",
            IsActive = true
        };

        db.ScreenDefinitions.Add(screen);

        db.ColumnDefinitions.AddRange(
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "employeeCode",
                DisplayLabel = "Employee Code",
                DataType = "text",
                DisplayOrder = 1,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "fullName",
                DisplayLabel = "Full Name",
                DataType = "text",
                DisplayOrder = 2,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "department",
                DisplayLabel = "Department",
                DataType = "text",
                DisplayOrder = 3,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "jobTitle",
                DisplayLabel = "Job Title",
                DataType = "text",
                DisplayOrder = 4,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "email",
                DisplayLabel = "Email",
                DataType = "text",
                DisplayOrder = 5,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "status",
                DisplayLabel = "Status",
                DataType = "text",
                DisplayOrder = 6,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = true
            },
            new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "createdDate",
                DisplayLabel = "Created Date",
                DataType = "date",
                DisplayOrder = 7,
                IsVisible = true,
                IsSortable = true,
                IsSearchable = false
            }
        );

        db.FormFieldDefinitions.AddRange(
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "employeeCode",
                DisplayLabel = "Employee Code",
                ControlType = "textbox",
                DataType = "text",
                DisplayOrder = 1,
                IsRequired = true,
                IsVisible = true,
                MaxLength = 50,
                Placeholder = "Enter employee code"
            },
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "fullName",
                DisplayLabel = "Full Name",
                ControlType = "textbox",
                DataType = "text",
                DisplayOrder = 2,
                IsRequired = true,
                IsVisible = true,
                MaxLength = 200,
                Placeholder = "Enter full name"
            },
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "department",
                DisplayLabel = "Department",
                ControlType = "dropdown",
                DataType = "text",
                DisplayOrder = 3,
                IsRequired = true,
                IsVisible = true
            },
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "jobTitle",
                DisplayLabel = "Job Title",
                ControlType = "textbox",
                DataType = "text",
                DisplayOrder = 4,
                IsRequired = true,
                IsVisible = true,
                MaxLength = 150
            },
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "email",
                DisplayLabel = "Email",
                ControlType = "textbox",
                DataType = "email",
                DisplayOrder = 5,
                IsRequired = true,
                IsVisible = true,
                MaxLength = 200
            },
            new FormFieldDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                FieldName = "status",
                DisplayLabel = "Status",
                ControlType = "dropdown",
                DataType = "text",
                DisplayOrder = 6,
                IsRequired = true,
                IsVisible = true
            }
        );

        db.ScreenPermissions.AddRange(
            new ScreenPermission
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                RoleName = "ADMIN",
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                CanExport = true
            },
            new ScreenPermission
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                RoleName = "MANAGER",
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = false,
                CanExport = true
            },
            new ScreenPermission
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                RoleName = "USER",
                CanView = true,
                CanAdd = false,
                CanEdit = false,
                CanDelete = false,
                CanExport = false
            },
            new ScreenPermission
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screen.Id,
                RoleName = "READONLY",
                CanView = true,
                CanAdd = false,
                CanEdit = false,
                CanDelete = false,
                CanExport = false
            }
        );

        await db.SaveChangesAsync();
    }
}