using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NessWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedKeyForFavoriteEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteEvents",
                columns: table => new
                {
                    UserFavoriteEventsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteEvents", x => x.UserFavoriteEventsId);
                    table.ForeignKey(
                        name: "FK_UserFavoriteEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteEvents_EventId",
                table: "UserFavoriteEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteEvents_UserId",
                table: "UserFavoriteEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteEvents");
        }
    }
}
