using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IskoWalkAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAtField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "WalkRequests",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "WalkRequests");
        }
    }
}
