using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTypeSizeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "configuraciones_backups_logs",
                type: "bigint",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "configuraciones_backups_logs",
                type: "int",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "0");
        }
    }
}
