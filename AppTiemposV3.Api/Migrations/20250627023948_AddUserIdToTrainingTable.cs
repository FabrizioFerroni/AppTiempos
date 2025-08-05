using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTrainingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trainings_categories_CategoryId",
                table: "trainings");

            migrationBuilder.DropForeignKey(
                name: "FK_trainings_requeriments_RequerimentId",
                table: "trainings");

            migrationBuilder.DropIndex(
                name: "IX_Training_Capacitator",
                table: "trainings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "trainings",
                type: "longtext",
                nullable: false,
                defaultValueSql: "En Progreso",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "trainings",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Capacitator_User",
                table: "trainings",
                columns: new[] { "Capacitator", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainings_UserId",
                table: "trainings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_trainings_categories_CategoryId",
                table: "trainings",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trainings_requeriments_RequerimentId",
                table: "trainings",
                column: "RequerimentId",
                principalTable: "requeriments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trainings_usuarios_UserId",
                table: "trainings",
                column: "UserId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trainings_categories_CategoryId",
                table: "trainings");

            migrationBuilder.DropForeignKey(
                name: "FK_trainings_requeriments_RequerimentId",
                table: "trainings");

            migrationBuilder.DropForeignKey(
                name: "FK_trainings_usuarios_UserId",
                table: "trainings");

            migrationBuilder.DropIndex(
                name: "IX_Training_Capacitator_User",
                table: "trainings");

            migrationBuilder.DropIndex(
                name: "IX_trainings_UserId",
                table: "trainings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "trainings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "trainings",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldDefaultValueSql: "En Progreso")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Capacitator",
                table: "trainings",
                column: "Capacitator");

            migrationBuilder.AddForeignKey(
                name: "FK_trainings_categories_CategoryId",
                table: "trainings",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_trainings_requeriments_RequerimentId",
                table: "trainings",
                column: "RequerimentId",
                principalTable: "requeriments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
