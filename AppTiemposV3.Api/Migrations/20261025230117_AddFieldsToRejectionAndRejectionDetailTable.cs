using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToRejectionAndRejectionDetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requeriments_UserId_FolderId",
                table: "requeriments");

            migrationBuilder.AddColumn<bool>(
                name: "BadRejected",
                table: "rechazos_detalles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReasonBadRequest",
                table: "rechazos_detalles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Tester",
                table: "rechazos_detalles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsResolveTesting",
                table: "rechazos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedDateTesting",
                table: "rechazos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tester",
                table: "rechazos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Requeriments_UserId_FolderId",
                table: "requeriments",
                columns: new[] { "UserId", "FolderId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requeriments_UserId_FolderId",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "BadRejected",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "ReasonBadRequest",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "IsResolveTesting",
                table: "rechazos");

            migrationBuilder.DropColumn(
                name: "ResolvedDateTesting",
                table: "rechazos");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "rechazos");

            migrationBuilder.CreateIndex(
                name: "IX_Requeriments_UserId_FolderId",
                table: "requeriments",
                columns: new[] { "UserId", "FolderId" },
                unique: true);
        }
    }
}
