using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleDashboardTemplateAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleDashboardTemplateAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardTemplateDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDashboardTemplateAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleDashboardTemplateAssignments_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleDashboardTemplateAssignments_DashboardTemplateDefinitio~",
                        column: x => x.DashboardTemplateDefinitionId,
                        principalTable: "DashboardTemplateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleDashboardTemplateAssignments_AppRoleId",
                table: "RoleDashboardTemplateAssignments",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDashboardTemplateAssignments_DashboardTemplateDefinitio~",
                table: "RoleDashboardTemplateAssignments",
                column: "DashboardTemplateDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleDashboardTemplateAssignments");
        }
    }
}
