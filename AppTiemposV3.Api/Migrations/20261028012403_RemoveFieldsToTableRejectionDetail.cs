using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsToTableRejectionDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualFixTime",
                table: "rechazos_detalles");

            migrationBuilder.DropColumn(
                name: "EstimatedFixTime",
                table: "rechazos_detalles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ActualFixTime",
                table: "rechazos_detalles",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedFixTime",
                table: "rechazos_detalles",
                type: "time",
                nullable: true);
        }
    }
}
