using Microsoft.EntityFrameworkCore.Migrations;

namespace ScalableTeaching.Migrations
{
    public partial class increasedMachineConfigurability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Memmory",
                table: "Machines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VCPU",
                table: "Machines",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Memmory",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "VCPU",
                table: "Machines");
        }
    }
}
