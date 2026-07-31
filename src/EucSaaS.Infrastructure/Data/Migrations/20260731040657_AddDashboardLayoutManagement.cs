using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardLayoutManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LayoutCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LayoutName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsShared = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DashboardLayoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardLayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardWidgetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GridRow = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    GridColumn = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    GridWidth = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    GridHeight = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardLayoutItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardLayoutItems_DashboardLayouts_DashboardLayoutId",
                        column: x => x.DashboardLayoutId,
                        principalTable: "DashboardLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardLayoutItems_DashboardWidgetDefinitions_DashboardWi~",
                        column: x => x.DashboardWidgetDefinitionId,
                        principalTable: "DashboardWidgetDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardLayoutItems_DashboardLayoutId_DashboardWidgetDefin~",
                table: "DashboardLayoutItems",
                columns: new[] { "DashboardLayoutId", "DashboardWidgetDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardLayoutItems_DashboardLayoutId_DisplayOrder",
                table: "DashboardLayoutItems",
                columns: new[] { "DashboardLayoutId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardLayoutItems_DashboardWidgetDefinitionId",
                table: "DashboardLayoutItems",
                column: "DashboardWidgetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardLayouts_TenantId_AppRoleId_DepartmentId_IsDefault_~",
                table: "DashboardLayouts",
                columns: new[] { "TenantId", "AppRoleId", "DepartmentId", "IsDefault", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardLayouts_TenantId_LayoutCode",
                table: "DashboardLayouts",
                columns: new[] { "TenantId", "LayoutCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardLayoutItems");

            migrationBuilder.DropTable(
                name: "DashboardLayouts");
        }
    }
}
