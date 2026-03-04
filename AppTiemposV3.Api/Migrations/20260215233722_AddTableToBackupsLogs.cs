using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTableToBackupsLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEmail",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationSemanal",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationMetaAlcanzada",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationDiario",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true,
                oldDefaultValueSql: "0");

            migrationBuilder.CreateTable(
                name: "configuraciones_backups_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Size = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    Type = table.Column<string>(type: "longtext", nullable: false, defaultValueSql: "Manual")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PathToBackup = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfigurationEntityId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_backups_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_configuraciones_backups_logs_configuraciones_ConfigurationEn~",
                        column: x => x.ConfigurationEntityId,
                        principalTable: "configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_configuraciones_backups_logs_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_backups_logs_ConfigurationEntityId",
                table: "configuraciones_backups_logs",
                column: "ConfigurationEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_backups_logs_UserId",
                table: "configuraciones_backups_logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuraciones_backups_logs");

            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEmail",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationSemanal",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationMetaAlcanzada",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<bool>(
                name: "EnableNotificationDiario",
                table: "configuraciones_notificaciones",
                type: "tinyint(1)",
                nullable: true,
                defaultValueSql: "0",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "0");
        }
    }
}
