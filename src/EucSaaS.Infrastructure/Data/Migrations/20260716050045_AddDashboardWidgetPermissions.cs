using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardWidgetPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardWidgetPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardWidgetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgetPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardWidgetPermissions_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardWidgetPermissions_DashboardWidgetDefinitions_Dashb~",
                        column: x => x.DashboardWidgetDefinitionId,
                        principalTable: "DashboardWidgetDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetPermissions_AppRoleId",
                table: "DashboardWidgetPermissions",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetPermissions_DashboardWidgetDefinitionId_AppR~",
                table: "DashboardWidgetPermissions",
                columns: new[] { "DashboardWidgetDefinitionId", "AppRoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardWidgetPermissions");
        }
    }
}
