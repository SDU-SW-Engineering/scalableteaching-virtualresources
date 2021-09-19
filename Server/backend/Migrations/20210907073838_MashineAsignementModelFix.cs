using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ScalableTeaching.Migrations
{
    public partial class MashineAsignementModelFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastPoll",
                table: "MachineStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "MachineCpuUtilizationPercent",
                table: "MachineStatuses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MachineIp",
                table: "MachineStatuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineLCMState",
                table: "MachineStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MachineMac",
                table: "MachineStatuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineMemmoryUtilizationBytes",
                table: "MachineStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MachineState",
                table: "MachineStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPoll",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineCpuUtilizationPercent",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineIp",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineLCMState",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineMac",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineMemmoryUtilizationBytes",
                table: "MachineStatuses");

            migrationBuilder.DropColumn(
                name: "MachineState",
                table: "MachineStatuses");
        }
    }
}
