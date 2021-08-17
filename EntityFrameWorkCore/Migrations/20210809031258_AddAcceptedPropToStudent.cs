using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.EntityFrameWorkCore.Migrations
{
    public partial class AddAcceptedPropToStudent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "Students");
        }
    }
}
