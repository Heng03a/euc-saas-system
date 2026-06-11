using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    public partial class AddTableNameToScreenDefinition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "ScreenDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TableName",
                table: "ScreenDefinitions");
        }
    }
}
