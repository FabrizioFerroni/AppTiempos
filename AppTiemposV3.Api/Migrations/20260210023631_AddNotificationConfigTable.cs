using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NotificationConfigId",
                table: "configuraciones",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "configuraciones_notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EnableNotificationDiario = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0"),
                    EnableNotificationSemanal = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0"),
                    EnableNotificationMetaAlcanzada = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0"),
                    NotificationsEmail = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0"),
                    HoraNotificacionDiaria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_notificaciones", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_NotificationConfigId",
                table: "configuraciones",
                column: "NotificationConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones",
                column: "NotificationConfigId",
                principalTable: "configuraciones_notificaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones");

            migrationBuilder.DropTable(
                name: "configuraciones_notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_configuraciones_NotificationConfigId",
                table: "configuraciones");

            migrationBuilder.DropColumn(
                name: "NotificationConfigId",
                table: "configuraciones");
        }
    }
}
