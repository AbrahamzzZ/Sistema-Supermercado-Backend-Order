using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Domain.Migrations.Scripts;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYProcedimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_VENTA",
                table: "VENTA",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "LOG",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_REGISTRO",
                table: "DETALLE_VENTA",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_REGISTRO",
                table: "DETALLE_COMPRA",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_COMPRA",
                table: "COMPRA",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            // Procedimientos almacenados y tipos tabla (scripts embebidos en Migrations/Scripts)
            migrationBuilder.EjecutarScript("V001_TiposTabla.sql");
            migrationBuilder.EjecutarScript("V001_Compra.sql");
            migrationBuilder.EjecutarScript("V001_Venta.sql");
            migrationBuilder.EjecutarScript("V001_Log.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_OBTENER_COMPRA, PA_OBTENER_DETALLES_COMPRA, PA_REGISTRAR_COMPRA;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_OBTENER_VENTA, PA_OBTENER_DETALLES_VENTA, PA_REGISTRAR_VENTA;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS PA_LISTA_LOG_PAGINACION, PA_OBTENER_LOG, PA_REGISTRAR_LOG;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS dbo.TI_Detalle_Compra;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS dbo.TI_Detalle_Venta;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_VENTA",
                table: "VENTA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA",
                table: "LOG",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_REGISTRO",
                table: "DETALLE_VENTA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_REGISTRO",
                table: "DETALLE_COMPRA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FECHA_COMPRA",
                table: "COMPRA",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");
        }
    }
}
