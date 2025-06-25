using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpFinalCripto.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClienteIdATransaccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CryptoAmount",
                table: "Transacciones",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId1",
                table: "Transacciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_ClienteId1",
                table: "Transacciones",
                column: "ClienteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacciones_Clientes_ClienteId1",
                table: "Transacciones",
                column: "ClienteId1",
                principalTable: "Clientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacciones_Clientes_ClienteId1",
                table: "Transacciones");

            migrationBuilder.DropIndex(
                name: "IX_Transacciones_ClienteId1",
                table: "Transacciones");

            migrationBuilder.DropColumn(
                name: "ClienteId1",
                table: "Transacciones");

            migrationBuilder.AlterColumn<decimal>(
                name: "CryptoAmount",
                table: "Transacciones",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);
        }
    }
}
