using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardWidgetLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColumnPosition",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowPosition",
                table: "DashboardWidgetDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnPosition",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "DashboardWidgetDefinitions");

            migrationBuilder.DropColumn(
                name: "RowPosition",
                table: "DashboardWidgetDefinitions");
        }
    }
}
