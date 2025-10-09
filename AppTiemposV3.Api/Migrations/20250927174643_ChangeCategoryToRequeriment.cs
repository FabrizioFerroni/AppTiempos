using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCategoryToRequeriment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_categories_CategoryId",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_categories_CategoryId",
                table: "trainings");

            migrationBuilder.DropIndex(
                name: "IX_activities_CategoryId",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "trainings");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "activities");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "requeriments",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_requeriments_CategoryId",
                table: "requeriments",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_requeriments_categories_CategoryId",
                table: "requeriments",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_requeriments_categories_CategoryId",
                table: "requeriments");

            migrationBuilder.DropIndex(
                name: "IX_requeriments_CategoryId",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "requeriments");
        }
    }
}
