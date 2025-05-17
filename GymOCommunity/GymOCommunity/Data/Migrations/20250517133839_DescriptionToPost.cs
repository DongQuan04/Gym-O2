using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymOCommunity.Data.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Posts");
        }
    }
}
