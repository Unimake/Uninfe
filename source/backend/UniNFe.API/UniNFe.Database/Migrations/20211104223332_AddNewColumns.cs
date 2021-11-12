using Microsoft.EntityFrameworkCore.Migrations;

namespace UniNFe.Database.Migrations
{
    public partial class AddNewColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Csc",
                table: "Configurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "Configurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdToken",
                table: "Configurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuanceType",
                table: "Configurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UF",
                table: "Configurations",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Csc",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "IdToken",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "IssuanceType",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "UF",
                table: "Configurations");
        }
    }
}
