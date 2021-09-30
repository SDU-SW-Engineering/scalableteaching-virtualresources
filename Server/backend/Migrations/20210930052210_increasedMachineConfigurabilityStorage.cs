using Microsoft.EntityFrameworkCore.Migrations;

namespace ScalableTeaching.Migrations
{
    public partial class increasedMachineConfigurabilityStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Memmory",
                table: "Machines",
                newName: "Storage");

            migrationBuilder.AddColumn<int>(
                name: "Memory",
                table: "Machines",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Memory",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "Storage",
                table: "Machines",
                newName: "Memmory");
        }
    }
}
