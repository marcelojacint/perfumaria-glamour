using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glamour.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracaoHero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracao_hero",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Eyebrow = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    TituloDestaque = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Subtitulo = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    CorDestaque = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CorTexto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImagemFundoUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracao_hero", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracao_hero");
        }
    }
}
