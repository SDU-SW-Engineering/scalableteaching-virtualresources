using Microsoft.EntityFrameworkCore.Migrations;

namespace ScalableTeaching.Migrations
{
    public partial class userkeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPublicKey",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserPublicKey",
                table: "Users");
        }
    }
}
