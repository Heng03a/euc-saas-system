using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupCodeToFormFieldDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LookupCode",
                table: "FormFieldDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LookupDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LookupCode = table.Column<string>(type: "text", nullable: false),
                    LookupName = table.Column<string>(type: "text", nullable: false),
                    SqlQuery = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupDefinitions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupDefinitions");

            migrationBuilder.DropColumn(
                name: "LookupCode",
                table: "FormFieldDefinitions");
        }
    }
}
