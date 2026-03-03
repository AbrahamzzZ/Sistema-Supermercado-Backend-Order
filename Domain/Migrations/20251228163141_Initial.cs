using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "COMPRA",
                columns: table => new
                {
                    ID_COMPRA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_USUARIO = table.Column<int>(type: "int", nullable: true),
                    ID_SUCURSAL = table.Column<int>(type: "int", nullable: true),
                    ID_PROVEEDOR = table.Column<int>(type: "int", nullable: true),
                    ID_TRANSPORTISTA = table.Column<int>(type: "int", nullable: true),
                    TIPO_DOCUMENTO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    NUMERO_DOCUMENTO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MONTO_TOTAL = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FECHA_COMPRA = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMPRA", x => x.ID_COMPRA);
                });

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
                    FECHA = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ENDPOINT = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    METODO = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NIVEL = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "ERROR")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOG", x => x.ID_LOG);
                });

            migrationBuilder.CreateTable(
                name: "VENTA",
                columns: table => new
                {
                    ID_VENTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_USUARIO = table.Column<int>(type: "int", nullable: true),
                    ID_SUCURSAL = table.Column<int>(type: "int", nullable: true),
                    ID_CLIENTE = table.Column<int>(type: "int", nullable: true),
                    TIPO_DOCUMENTO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    NUMERO_DOCUMENTO = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MONTO_PAGO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MONTO_CAMBIO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MONTO_TOTAL = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DESCUENTO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FECHA_VENTA = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VENTA", x => x.ID_VENTA);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_COMPRA",
                columns: table => new
                {
                    ID_DETALLE_COMPRA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_COMPRA = table.Column<int>(type: "int", nullable: false),
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: true),
                    PRECIO_COMPRA = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    PRECIO_VENTA = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    CANTIDAD = table.Column<int>(type: "int", nullable: true),
                    SUBTOTAL = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_COMPRA", x => x.ID_DETALLE_COMPRA);
                    table.ForeignKey(
                        name: "FK_DETALLE_COMPRA_COMPRA_ID_COMPRA",
                        column: x => x.ID_COMPRA,
                        principalTable: "COMPRA",
                        principalColumn: "ID_COMPRA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_VENTA",
                columns: table => new
                {
                    ID_DETALLE_VENTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_VENTA = table.Column<int>(type: "int", nullable: false),
                    ID_PRODUCTO = table.Column<int>(type: "int", nullable: true),
                    PRECIO_VENTA = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CANTIDAD = table.Column<int>(type: "int", nullable: true),
                    SUBTOTAL = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DESCUENTO = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_VENTA", x => x.ID_DETALLE_VENTA);
                    table.ForeignKey(
                        name: "FK_DETALLE_VENTA_VENTA_ID_VENTA",
                        column: x => x.ID_VENTA,
                        principalTable: "VENTA",
                        principalColumn: "ID_VENTA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__COMPRA__87B6EC7EAF707742",
                table: "COMPRA",
                column: "NUMERO_DOCUMENTO",
                unique: true,
                filter: "[NUMERO_DOCUMENTO] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_COMPRA_ID_COMPRA",
                table: "DETALLE_COMPRA",
                column: "ID_COMPRA");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_VENTA_ID_VENTA",
                table: "DETALLE_VENTA",
                column: "ID_VENTA");

            migrationBuilder.CreateIndex(
                name: "UQ__VENTA__87B6EC7E76F1FEDA",
                table: "VENTA",
                column: "NUMERO_DOCUMENTO",
                unique: true,
                filter: "[NUMERO_DOCUMENTO] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DETALLE_COMPRA");

            migrationBuilder.DropTable(
                name: "DETALLE_VENTA");

            migrationBuilder.DropTable(
                name: "LOG");

            migrationBuilder.DropTable(
                name: "COMPRA");

            migrationBuilder.DropTable(
                name: "VENTA");
        }
    }
}
