using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glamour.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFonteTitulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FonteTitulo",
                table: "configuracao_hero",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FonteTitulo",
                table: "configuracao_hero");
        }
    }
}
