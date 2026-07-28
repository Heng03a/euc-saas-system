using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardWidgetTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardWidgetTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultWidgetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultSqlQuery = table.Column<string>(type: "text", nullable: false),
                    DefaultIcon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultGridWidth = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    DefaultGridHeight = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgetTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetTemplates_IsSystem_IsActive",
                table: "DashboardWidgetTemplates",
                columns: new[] { "IsSystem", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetTemplates_TenantId_IsActive_Category",
                table: "DashboardWidgetTemplates",
                columns: new[] { "TenantId", "IsActive", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetTemplates_TenantId_TemplateCode",
                table: "DashboardWidgetTemplates",
                columns: new[] { "TenantId", "TemplateCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardWidgetTemplates");
        }
    }
}
