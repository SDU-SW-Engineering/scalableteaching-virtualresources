using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ScalableTeaching.Migrations
{
    public partial class MachineDeletionRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineDeletionRequests",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserUsername = table.Column<string>(type: "text", nullable: true),
                    MachineID1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDeletionRequests", x => x.MachineID);
                    table.ForeignKey(
                        name: "FK_MachineDeletionRequests_Machines_MachineID1",
                        column: x => x.MachineID1,
                        principalTable: "Machines",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineDeletionRequests_Users_UserUsername",
                        column: x => x.UserUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineDeletionRequests_MachineID1",
                table: "MachineDeletionRequests",
                column: "MachineID1");

            migrationBuilder.CreateIndex(
                name: "IX_MachineDeletionRequests_UserUsername",
                table: "MachineDeletionRequests",
                column: "UserUsername");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineDeletionRequests");
        }
    }
}
