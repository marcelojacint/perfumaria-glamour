using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glamour.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaNewsletterENotificacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "newsletters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notificacoes_estoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notificado = table.Column<bool>(type: "boolean", nullable: false),
                    NotificadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes_estoque", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_newsletters_Email",
                table: "newsletters",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_estoque_ProdutoId_Email",
                table: "notificacoes_estoque",
                columns: new[] { "ProdutoId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsletters");

            migrationBuilder.DropTable(
                name: "notificacoes_estoque");
        }
    }
}
