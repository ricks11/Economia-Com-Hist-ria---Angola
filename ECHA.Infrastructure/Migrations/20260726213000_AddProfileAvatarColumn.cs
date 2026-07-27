using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EconomiaComHistoria.Infrastructure.Migrations;

/// <summary>Armazena o avatar codificado do utilizador, usado pelo perfil mobile.</summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260726213000_AddProfileAvatarColumn")]
public partial class AddProfileAvatarColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AvatarConfig",
            table: "Utilizadores",
            type: "longtext",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AvatarConfig", table: "Utilizadores");
    }
}
