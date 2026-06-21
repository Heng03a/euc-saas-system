using EucSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
      {
            
      }

      public DbSet<DataSource> DataSources { get; set; }
      
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppMenu> AppMenus => Set<AppMenu>();

public DbSet<LookupDefinition> LookupDefinitions => Set<LookupDefinition>();

    public DbSet<ScreenDefinition> ScreenDefinitions => Set<ScreenDefinition>();
    public DbSet<ColumnDefinition> ColumnDefinitions => Set<ColumnDefinition>();
    public DbSet<FormFieldDefinition> FormFieldDefinitions => Set<FormFieldDefinition>();
    public DbSet<ScreenPermission> ScreenPermissions => Set<ScreenPermission>();

    public DbSet<FormFieldOptionDefinition> FormFieldOptionDefinitions => Set<FormFieldOptionDefinition>();
    public DbSet<Employee> Employees => Set<Employee>();  

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

      //-----------------------------------
      // Employee
      //-----------------------------------
      builder.Entity<Employee>(entity =>
      {
      entity.HasKey(x => x.Id);

      entity.Property(x => x.EmployeeCode)
            .HasMaxLength(50)
            .IsRequired();

      entity.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

      entity.Property(x => x.Department)
            .HasMaxLength(100)
            .IsRequired();

      entity.Property(x => x.JobTitle)
            .HasMaxLength(150)
            .IsRequired();

      entity.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();

      entity.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

      entity.HasIndex(x => x.EmployeeCode)
            .IsUnique();
      });

        //-----------------------------------
        // Tenant
        //-----------------------------------
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                  .HasMaxLength(200)
                  .IsRequired();
        });

        //-----------------------------------
        // Department
        //-----------------------------------
        builder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                  .HasMaxLength(200)
                  .IsRequired();
        });

        //-----------------------------------
        // Role
        //-----------------------------------
        builder.Entity<AppRole>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                  .HasMaxLength(100)
                  .IsRequired();
        });

        //-----------------------------------
        // User
        //-----------------------------------
        builder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(x => x.PasswordHash)
                  .HasMaxLength(500)
                  .IsRequired();

            entity.Property(x => x.FullName)
                  .HasMaxLength(200);

            entity.Property(x => x.Email)
                  .HasMaxLength(200);

            //-----------------------------------
            // Relationships
            //-----------------------------------

            entity.HasOne(x => x.Role)
                  .WithMany()
                  .HasForeignKey(x => x.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                  .WithMany()
                  .HasForeignKey(x => x.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Tenant)
                  .WithMany()
                  .HasForeignKey(x => x.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        //-----------------------------------
        // Menu
        //-----------------------------------
        builder.Entity<AppMenu>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(x => x.Url)
                  .HasMaxLength(500);

            entity.Property(x => x.Icon)
                  .HasMaxLength(100);
        });

        //-----------------------------------
        // Screen Definition
        //-----------------------------------
        builder.Entity<ScreenDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScreenCode)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.ScreenName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.EntityName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.RoutePath)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.Description)
                  .HasMaxLength(500);

            entity.HasIndex(x => x.ScreenCode)
                  .IsUnique();
        });

        //-----------------------------------
        // Column Definition
        //-----------------------------------
        builder.Entity<ColumnDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FieldName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.DisplayLabel)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.DataType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasOne(x => x.ScreenDefinition)
                  .WithMany(x => x.Columns)
                  .HasForeignKey(x => x.ScreenDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        //-----------------------------------
        // Form Field Definition
        //-----------------------------------
        builder.Entity<FormFieldDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FieldName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.DisplayLabel)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.ControlType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(x => x.DataType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(x => x.Placeholder)
                  .HasMaxLength(200);

            entity.HasOne(x => x.ScreenDefinition)
                  .WithMany(x => x.FormFields)
                  .HasForeignKey(x => x.ScreenDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

//-----------------------------------
// Form Field Option Definition
//-----------------------------------
builder.Entity<FormFieldOptionDefinition>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.Property(x => x.OptionLabel)
          .IsRequired()
          .HasMaxLength(200);

    entity.Property(x => x.OptionValue)
          .IsRequired()
          .HasMaxLength(200);

    entity.HasOne(x => x.FormFieldDefinition)
          .WithMany(x => x.Options)
          .HasForeignKey(x => x.FormFieldDefinitionId)
          .OnDelete(DeleteBehavior.Cascade);
});

        //-----------------------------------
        // Screen Permission
        //-----------------------------------
        builder.Entity<ScreenPermission>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasOne(x => x.ScreenDefinition)
                  .WithMany(x => x.Permissions)
                  .HasForeignKey(x => x.ScreenDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ScreenDefinitionId, x.RoleName })
                  .IsUnique();
        });

    }
}
