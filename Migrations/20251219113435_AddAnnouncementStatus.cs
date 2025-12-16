using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Topluluk_Yonetim.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Announcements",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Announcements");
        }
    }
}
