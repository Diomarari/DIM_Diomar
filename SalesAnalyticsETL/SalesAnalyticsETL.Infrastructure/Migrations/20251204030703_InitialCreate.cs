using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SalesAnalyticsETL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dim_Cliente",
                columns: table => new
                {
                    ClienteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Cliente", x => x.ClienteID);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Estado",
                columns: table => new
                {
                    EstadoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Estado", x => x.EstadoID);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Producto",
                columns: table => new
                {
                    ProductoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Producto", x => x.ProductoID);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Tiempo",
                columns: table => new
                {
                    TiempoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dia = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Trimestre = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    NombreMes = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EsFinDeSemana = table.Column<bool>(type: "bit", nullable: false),
                    EsFeriado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Tiempo", x => x.TiempoID);
                });

            migrationBuilder.CreateTable(
                name: "Fact_Ventas",
                columns: table => new
                {
                    VentaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    ProductoID = table.Column<int>(type: "int", nullable: false),
                    TiempoID = table.Column<int>(type: "int", nullable: false),
                    EstadoID = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fact_Ventas", x => x.VentaID);
                    table.ForeignKey(
                        name: "FK_Fact_Ventas_Dim_Cliente_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Dim_Cliente",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fact_Ventas_Dim_Estado_EstadoID",
                        column: x => x.EstadoID,
                        principalTable: "Dim_Estado",
                        principalColumn: "EstadoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fact_Ventas_Dim_Producto_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Dim_Producto",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fact_Ventas_Dim_Tiempo_TiempoID",
                        column: x => x.TiempoID,
                        principalTable: "Dim_Tiempo",
                        principalColumn: "TiempoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dim_Estado",
                columns: new[] { "EstadoID", "Activo", "Descripcion", "FechaActualizacion", "FechaCreacion", "NombreEstado" },
                values: new object[,]
                {
                    { 1, true, "Orden pendiente de procesamiento", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PENDIENTE" },
                    { 2, true, "Orden en proceso", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PROCESANDO" },
                    { 3, true, "Orden completada exitosamente", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "COMPLETADO" },
                    { 4, true, "Orden cancelada", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CANCELADO" },
                    { 5, true, "Orden devuelta", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DEVUELTO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Cliente_Email",
                table: "Dim_Cliente",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Producto_Categoria",
                table: "Dim_Producto",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Producto_Nombre",
                table: "Dim_Producto",
                column: "NombreProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Tiempo_AnioMes",
                table: "Dim_Tiempo",
                columns: new[] { "Anio", "Mes" });

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Tiempo_Fecha",
                table: "Dim_Tiempo",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_ClienteID",
                table: "Fact_Ventas",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_Dimensions",
                table: "Fact_Ventas",
                columns: new[] { "ClienteID", "ProductoID", "TiempoID" });

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_EstadoID",
                table: "Fact_Ventas",
                column: "EstadoID");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_OrdenID",
                table: "Fact_Ventas",
                column: "OrdenID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_ProductoID",
                table: "Fact_Ventas",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_Ventas_TiempoID",
                table: "Fact_Ventas",
                column: "TiempoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fact_Ventas");

            migrationBuilder.DropTable(
                name: "Dim_Cliente");

            migrationBuilder.DropTable(
                name: "Dim_Estado");

            migrationBuilder.DropTable(
                name: "Dim_Producto");

            migrationBuilder.DropTable(
                name: "Dim_Tiempo");
        }
    }
}
