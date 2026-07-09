using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardGridLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GridColumn",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GridHeight",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GridRow",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GridWidth",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GridColumn",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "GridHeight",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "GridRow",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "GridWidth",
                table: "DashboardWidgetDefinitions");
        }
    }
}
