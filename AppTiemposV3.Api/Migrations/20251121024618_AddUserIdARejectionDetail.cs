using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdARejectionDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "rechazos_detalles",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_rechazos_detalles_UserId",
                table: "rechazos_detalles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_rechazos_detalles_usuarios_UserId",
                table: "rechazos_detalles",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rechazos_detalles_usuarios_UserId",
                table: "rechazos_detalles");

            migrationBuilder.DropIndex(
                name: "IX_rechazos_detalles_UserId",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "rechazos_detalles");
        }
    }
}
