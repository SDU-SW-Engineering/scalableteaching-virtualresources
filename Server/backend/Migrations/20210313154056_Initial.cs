using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    Mail = table.Column<string>(type: "text", nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    UserPrivateKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CouseID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserUsername = table.Column<string>(type: "text", nullable: false, comment: "The user responsible for the course i.e. the user that can make machines associated with the course"),
                    CourseName = table.Column<string>(type: "text", nullable: false),
                    ShortCourseName = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, comment: "Should be between 3 and 6 characters"),
                    SDUCourseID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CouseID);
                    table.ForeignKey(
                        name: "FK_Courses_Users_UserUsername",
                        column: x => x.UserUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupID = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "text", nullable: true),
                    CourseID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupID);
                    table.ForeignKey(
                        name: "FK_Groups_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CouseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    HostName = table.Column<string>(type: "text", nullable: true),
                    UserUsername = table.Column<string>(type: "text", nullable: false),
                    CourseID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineID);
                    table.ForeignKey(
                        name: "FK_Machines_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CouseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Machines_Users_UserUsername",
                        column: x => x.UserUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupAssignments",
                columns: table => new
                {
                    GroupID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserUsername = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAssignments", x => new { x.GroupID, x.UserUsername });
                    table.ForeignKey(
                        name: "FK_GroupAssignments_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAssignments_Users_UserUsername",
                        column: x => x.UserUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "MachineAssignments",
                columns: table => new
                {
                    MachineID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserUsername = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineCredentials", x => new { x.MachineID, x.UserUsername });
                    table.ForeignKey(
                        name: "FK_MachineCredentials_Machines_MachineID",
                        column: x => x.MachineID,
                        principalTable: "Machines",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MachineCredentials_Users_UserUsername",
                        column: x => x.UserUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserUsername",
                table: "Courses",
                column: "UserUsername");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAssignments_UserUsername",
                table: "GroupAssignments",
                column: "UserUsername");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CourseID",
                table: "Groups",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_MachineCredentials_UserUsername",
                table: "MachineAssignments",
                column: "UserUsername");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CourseID",
                table: "Machines",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_UserUsername",
                table: "Machines",
                column: "UserUsername");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupAssignments");

            migrationBuilder.DropTable(
                name: "LocalForwards");

            migrationBuilder.DropTable(
                name: "MachineAssignments");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
