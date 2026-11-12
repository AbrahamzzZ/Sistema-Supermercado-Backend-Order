using System;
using Domain.Migrations.Scripts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <summary>
    /// Elimina la tabla LOG y sus procedimientos: los logs de todos los microservicios se guardan en Seq
    /// (con retención automática) y la pantalla "Log" de la app los lee desde Admin (/api/Log).
    /// </summary>
    public partial class EliminarLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_LISTA_LOG_PAGINACION;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_OBTENER_LOG;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_REGISTRAR_LOG;");

            migrationBuilder.DropTable(name: "LOG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Misma tabla que dejaron las migraciones Initial y AuditoriaYProcedimientos
            migrationBuilder.CreateTable(
                name: "LOG",
                columns: table => new
                {
                    ID_LOG = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CODIGO_ERROR = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    MENSAJE_ERROR = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    DETALLE_ERROR = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ID_USUARIO = table.Column<int>(type: "int", nullable: true),
                    FECHA = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    ENDPOINT = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    METODO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NIVEL = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "ERROR")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOG", x => x.ID_LOG);
                });

            migrationBuilder.EjecutarScript("V001_Log.sql");
        }
    }
}
