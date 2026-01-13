using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IskoWalkAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "WalkRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedBy",
                table: "WalkRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "WalkRequests");

            migrationBuilder.DropColumn(
                name: "AcceptedBy",
                table: "WalkRequests");
        }
    }
}
