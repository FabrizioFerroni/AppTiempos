using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTesterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tester",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "Tester",
                table: "rechazos");

            migrationBuilder.AddColumn<Guid>(
                name: "TesterId",
                table: "rechazos_detalles",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TesterId",
                table: "rechazos",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "testers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rechazos_detalles_TesterId",
                table: "rechazos_detalles",
                column: "TesterId");

            migrationBuilder.CreateIndex(
                name: "IX_rechazos_TesterId",
                table: "rechazos",
                column: "TesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_rechazos_testers_TesterId",
                table: "rechazos",
                column: "TesterId",
                principalTable: "testers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rechazos_detalles_testers_TesterId",
                table: "rechazos_detalles",
                column: "TesterId",
                principalTable: "testers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rechazos_testers_TesterId",
                table: "rechazos");

            migrationBuilder.DropForeignKey(
                name: "FK_rechazos_detalles_testers_TesterId",
                table: "rechazos_detalles");

            migrationBuilder.DropTable(
                name: "testers");

            migrationBuilder.DropIndex(
                name: "IX_rechazos_detalles_TesterId",
                table: "rechazos_detalles");

            migrationBuilder.DropIndex(
                name: "IX_rechazos_TesterId",
                table: "rechazos");

            migrationBuilder.DropColumn(
                name: "TesterId",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "TesterId",
                table: "rechazos");

            migrationBuilder.AddColumn<string>(
                name: "Tester",
                table: "rechazos_detalles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Tester",
                table: "rechazos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
