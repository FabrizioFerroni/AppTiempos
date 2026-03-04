using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "usuarios",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<sbyte>(
                name: "IsAccountConfigurated",
                table: "usuarios",
                type: "tinyint",
                nullable: false,
                defaultValueSql: "0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "IsAccountConfigurated",
                table: "usuarios");
        }
    }
}
