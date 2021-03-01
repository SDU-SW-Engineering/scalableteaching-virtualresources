using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace backend.Migrations
{
    public partial class updatedportnaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.AddColumn<string>(
                name: "ShortCourseName",
                table: "Courses",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "",
                comment: "Should be between 3 and 6 characters");

            migrationBuilder.CreateTable(
                name: "LocalForwards",
                columns: table => new
                {
                    PortNumber = table.Column<int>(type: "integer", nullable: false),
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalForwards", x => new { x.MachineID, x.PortNumber });
                    table.ForeignKey(
                        name: "FK_LocalForwards_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalForwards");

            migrationBuilder.DropColumn(
                name: "ShortCourseName",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false),
                    PortNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => new { x.MachineID, x.PortNumber });
                    table.ForeignKey(
                        name: "FK_Ports_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
