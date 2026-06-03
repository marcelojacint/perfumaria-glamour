using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glamour.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampanhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campanhas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Subtitulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ImagemUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Link = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanhas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_campanhas_Ativa_Ordem",
                table: "campanhas",
                columns: new[] { "Ativa", "Ordem" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campanhas");
        }
    }
}
