using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <summary>
    /// Procedimientos de estadísticas de compras y ventas (V002_Estadisticas.sql) que usa Admin para sus reportes.
    /// No cambia tablas.
    /// </summary>
    public partial class Estadisticas : Migration
    {
        private static readonly string[] Procedimientos =
        {
            "PA_ESTADISTICA_PRODUCTOS_COMPRADOS", "PA_ESTADISTICA_PRODUCTOS_VENDIDOS",
            "PA_ESTADISTICA_TOP_CLIENTES", "PA_ESTADISTICA_TOP_PROVEEDORES",
            "PA_ESTADISTICA_VIAJES_TRANSPORTISTA", "PA_ESTADISTICA_VENTAS_EMPLEADO",
            "PA_ESTADISTICA_TENDENCIA_VENDIDOS", "PA_ESTADISTICA_TENDENCIA_COMPRADOS"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EjecutarScript("V002_Estadisticas.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var sp in Procedimientos)
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS {sp};");
        }
    }
}
