using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glamour.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPagamentoDividido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetodoPagamentoPromocao",
                table: "pedidos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorEmPromocao",
                table: "pedidos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetodoPagamentoPromocao",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "ValorEmPromocao",
                table: "pedidos");
        }
    }
}
