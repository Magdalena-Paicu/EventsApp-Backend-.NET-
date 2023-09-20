﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NessWebApi.Migrations
{
    /// <inheritdoc />
    public partial class addedIsFavoriteProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isFavorite",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isFavorite",
                table: "Events");
        }
    }
}
