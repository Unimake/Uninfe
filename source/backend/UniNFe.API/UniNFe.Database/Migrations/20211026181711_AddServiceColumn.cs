using Microsoft.EntityFrameworkCore.Migrations;

namespace UniNFe.Database.Migrations
{
    public partial class AddServiceColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "Configurations",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Service",
                table: "Configurations");
        }
    }
}
