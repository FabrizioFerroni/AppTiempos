using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToRequerimentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConjuntoCambios",
                table: "requeriments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql($@"
                ALTER TABLE `requeriments`
                ADD COLUMN `FolderId` INT NOT NULL AUTO_INCREMENT UNIQUE;
            ");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "requeriments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConjuntoCambios",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "FolderPath",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "requeriments");
        }
    }
}
