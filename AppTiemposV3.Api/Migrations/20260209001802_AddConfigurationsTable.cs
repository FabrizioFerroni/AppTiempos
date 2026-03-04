using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTiemposV3.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuraciones_horarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_horarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "configuraciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActualConfig = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1"),
                    AutoBackupEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "0"),
                    BackupFrecuencia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackupTime = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackupRetention = table.Column<int>(type: "int", nullable: true),
                    MaxBackup = table.Column<int>(type: "int", nullable: true),
                    WeeklyParId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WeeklyImparId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_configuraciones_configuraciones_horarios_WeeklyImparId",
                        column: x => x.WeeklyImparId,
                        principalTable: "configuraciones_horarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_configuraciones_configuraciones_horarios_WeeklyParId",
                        column: x => x.WeeklyParId,
                        principalTable: "configuraciones_horarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_configuraciones_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "configuraciones_dias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Day = table.Column<int>(type: "int", nullable: false),
                    DayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinHours = table.Column<double>(type: "double", nullable: false),
                    MaxHours = table.Column<double>(type: "double", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConfigurationEntityId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_dias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_configuraciones_dias_configuraciones_ConfigurationEntityId",
                        column: x => x.ConfigurationEntityId,
                        principalTable: "configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "configuraciones_sabados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ConfigurationEntityId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_sabados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_configuraciones_sabados_configuraciones_ConfigurationEntityId",
                        column: x => x.ConfigurationEntityId,
                        principalTable: "configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_UserId",
                table: "configuraciones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_WeeklyImparId",
                table: "configuraciones",
                column: "WeeklyImparId");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_WeeklyParId",
                table: "configuraciones",
                column: "WeeklyParId");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_dias_ConfigurationEntityId",
                table: "configuraciones_dias",
                column: "ConfigurationEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_configuraciones_sabados_ConfigurationEntityId",
                table: "configuraciones_sabados",
                column: "ConfigurationEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuraciones_dias");

            migrationBuilder.DropTable(
                name: "configuraciones_sabados");

            migrationBuilder.DropTable(
                name: "configuraciones");

            migrationBuilder.DropTable(
                name: "configuraciones_horarios");
        }
    }
}
