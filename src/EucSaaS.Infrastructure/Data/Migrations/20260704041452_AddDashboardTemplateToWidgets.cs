using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardTemplateToWidgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DashboardTemplateDefinitionId",
                table: "DashboardWidgetDefinitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetDefinitions_DashboardTemplateDefinitionId",
                table: "DashboardWidgetDefinitions",
                column: "DashboardTemplateDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DashboardWidgetDefinitions_DashboardTemplateDefinitions_Das~",
                table: "DashboardWidgetDefinitions",
                column: "DashboardTemplateDefinitionId",
                principalTable: "DashboardTemplateDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DashboardWidgetDefinitions_DashboardTemplateDefinitions_Das~",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_DashboardWidgetDefinitions_DashboardTemplateDefinitionId",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "DashboardTemplateDefinitionId",
                table: "DashboardWidgetDefinitions");
        }
    }
}
