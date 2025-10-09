using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "requeriments");

            migrationBuilder.AlterColumn<int>(
                name: "Area",
                table: "usuarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "requeriments",
                type: "int",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<int>(
                name: "Etapa",
                table: "activities",
                type: "int",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Etapa",
                table: "activities",
                column: "Etapa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_Etapa",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "requeriments");

            migrationBuilder.DropColumn(
                name: "Etapa",
                table: "activities");

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                table: "usuarios",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "requeriments",
                type: "int",
                nullable: true);
        }
    }
}
