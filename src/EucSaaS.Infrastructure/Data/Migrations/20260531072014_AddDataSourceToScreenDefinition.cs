using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    public partial class AddDataSourceToScreenDefinition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSourceCode = table.Column<string>(type: "text", nullable: false),
                    DataSourceName = table.Column<string>(type: "text", nullable: false),
                    DatabaseType = table.Column<string>(type: "text", nullable: false),
                    HostName = table.Column<string>(type: "text", nullable: false),
                    PortNumber = table.Column<int>(type: "integer", nullable: false),
                    DatabaseName = table.Column<string>(type: "text", nullable: false),
                    ReadOnlyUserName = table.Column<string>(type: "text", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

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

            migrationBuilder.AddColumn<string>(
                name: "Width",
                table: "ColumnDefinitions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSourceId",
                table: "ScreenDefinitions");

            migrationBuilder.DropColumn(
                name: "SchemaName",
                table: "ScreenDefinitions");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ColumnDefinitions");

            migrationBuilder.DropTable(
                name: "DataSources");
        }
    }
}
