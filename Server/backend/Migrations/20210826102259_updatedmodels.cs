using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ScalableTeaching.Migrations
{
    public partial class updatedmodels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalForwards");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Machines");

            migrationBuilder.AlterColumn<string>(
                name: "HostName",
                table: "Machines",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Apt",
                table: "Machines",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "LinuxGroups",
                table: "Machines",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineCreationStatus",
                table: "Machines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpenNebulaID",
                table: "Machines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "Ports",
                table: "Machines",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Ppa",
                table: "Machines",
                type: "text[]",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MachineStatuses",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStatuses", x => x.MachineID);
                    table.ForeignKey(
                        name: "FK_MachineStatuses_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "Apt",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "LinuxGroups",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineCreationStatus",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "OpenNebulaID",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Ports",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Ppa",
                table: "Machines");

            migrationBuilder.AlterColumn<string>(
                name: "HostName",
                table: "Machines",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Machines",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LocalForwards",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false),
                    PortNumber = table.Column<int>(type: "integer", nullable: false)
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
    }
}
