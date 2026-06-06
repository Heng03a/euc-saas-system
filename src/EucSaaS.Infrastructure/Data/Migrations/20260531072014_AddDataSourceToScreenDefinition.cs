using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSourceToScreenDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DataSourceId",
                table: "ScreenDefinitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchemaName",
                table: "ScreenDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSourceId",
                table: "ScreenDefinitions");

            migrationBuilder.DropColumn(
                name: "SchemaName",
                table: "ScreenDefinitions");
        }
    }
}
