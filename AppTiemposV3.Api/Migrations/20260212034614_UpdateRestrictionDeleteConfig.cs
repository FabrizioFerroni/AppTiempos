using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRestrictionDeleteConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyImparId",
                table: "configuraciones");

            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyParId",
                table: "configuraciones");

            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones");

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyImparId",
                table: "configuraciones",
                column: "WeeklyImparId",
                principalTable: "configuraciones_horarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyParId",
                table: "configuraciones",
                column: "WeeklyParId",
                principalTable: "configuraciones_horarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones",
                column: "NotificationConfigId",
                principalTable: "configuraciones_notificaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyImparId",
                table: "configuraciones");

            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyParId",
                table: "configuraciones");

            migrationBuilder.DropForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones");

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyImparId",
                table: "configuraciones",
                column: "WeeklyImparId",
                principalTable: "configuraciones_horarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_horarios_WeeklyParId",
                table: "configuraciones",
                column: "WeeklyParId",
                principalTable: "configuraciones_horarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_configuraciones_configuraciones_notificaciones_NotificationC~",
                table: "configuraciones",
                column: "NotificationConfigId",
                principalTable: "configuraciones_notificaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
