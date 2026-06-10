using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EucSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyColumnToScreenDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ScreenDefinitions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ScreenDefinitions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PrimaryKeyColumn",
                table: "ScreenDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                table: "FormFieldOptionDefinitions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OptionLabel",
                table: "FormFieldOptionDefinitions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenDefinitions_DataSourceId",
                table: "ScreenDefinitions",
                column: "DataSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScreenDefinitions_DataSources_DataSourceId",
                table: "ScreenDefinitions",
                column: "DataSourceId",
                principalTable: "DataSources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScreenDefinitions_DataSources_DataSourceId",
                table: "ScreenDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ScreenDefinitions_DataSourceId",
                table: "ScreenDefinitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ScreenDefinitions");

            migrationBuilder.DropColumn(
                name: "PrimaryKeyColumn",
                table: "ScreenDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ScreenDefinitions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                table: "FormFieldOptionDefinitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "OptionLabel",
                table: "FormFieldOptionDefinitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
