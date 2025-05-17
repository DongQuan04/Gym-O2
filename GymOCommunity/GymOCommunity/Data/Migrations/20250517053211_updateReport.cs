using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymOCommunity.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportedAt",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportedAt",
                table: "Reports");
        }
    }
}
