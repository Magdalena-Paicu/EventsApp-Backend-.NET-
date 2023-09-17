using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NessWebApi.Migrations
{
    /// <inheritdoc />
    public partial class deleteIsFavoriteFromEventModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isFavorite",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isFavorite",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
