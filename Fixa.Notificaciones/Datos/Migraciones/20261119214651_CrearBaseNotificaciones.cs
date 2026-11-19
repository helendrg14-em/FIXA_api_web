using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fixa.Notificaciones.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CrearBaseNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinatarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoReporte = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TokenDispositivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinatarios", x => x.Id);
                    table.CheckConstraint("CK_Destinatarios_AlgunContacto", "[Correo] IS NOT NULL OR [TokenDispositivo] IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "Plantillas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Cuerpo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantillas", x => x.Id);
                    table.CheckConstraint("CK_Plantillas_AsuntoDeCorreo", "[Canal] <> 'Correo' OR [Asunto] IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantillaId = table.Column<int>(type: "int", nullable: false),
                    DestinatarioId = table.Column<int>(type: "int", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Canal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Asunto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TokenDispositivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avisos_Destinatarios_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Destinatarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avisos_Plantillas_PlantillaId",
                        column: x => x.PlantillaId,
                        principalTable: "Plantillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntentosEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvisoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntentosEntrega_Avisos_AvisoId",
                        column: x => x.AvisoId,
                        principalTable: "Avisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Destinatarios",
                columns: new[] { "Id", "CodigoReporte", "Correo", "FechaRegistro", "TokenDispositivo" },
                values: new object[,]
                {
                    { 1, "FX-2026-000001", null, new DateTime(2026, 11, 19, 12, 0, 0, 0, DateTimeKind.Utc), "token-de-prueba-del-celular-0001" },
                    { 2, "FX-2026-000002", "vecino.prueba@fixa.test", new DateTime(2026, 11, 19, 12, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "FX-2026-000003", null, new DateTime(2026, 11, 19, 12, 0, 0, 0, DateTimeKind.Utc), "token-vencido" }
                });

            migrationBuilder.InsertData(
                table: "Plantillas",
                columns: new[] { "Id", "Activa", "Asunto", "Canal", "Codigo", "Cuerpo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, "Push", "caso_recibido", "Recibimos tu reporte {codigo}. Estado: {estado}.", "Reporte recibido" },
                    { 2, true, null, "Push", "caso_programado", "Tu reporte {codigo} ya tiene una cuadrilla asignada. Estado: {estado}.", "Atención programada" },
                    { 3, true, "Tu reporte en FIXA fue atendido", "Correo", "caso_cerrado", "El problema de tu reporte {codigo} fue solucionado. Estado: {estado}. Tienes 15 días para confirmar si quedaste conforme.", "Caso cerrado" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_DestinatarioId",
                table: "Avisos",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_Estado",
                table: "Avisos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_PlantillaId",
                table: "Avisos",
                column: "PlantillaId");

            migrationBuilder.CreateIndex(
                name: "IX_Destinatarios_CodigoReporte",
                table: "Destinatarios",
                column: "CodigoReporte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntentosEntrega_AvisoId",
                table: "IntentosEntrega",
                column: "AvisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Plantillas_Codigo",
                table: "Plantillas",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntentosEntrega");

            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropTable(
                name: "Destinatarios");

            migrationBuilder.DropTable(
                name: "Plantillas");
        }
    }
}
