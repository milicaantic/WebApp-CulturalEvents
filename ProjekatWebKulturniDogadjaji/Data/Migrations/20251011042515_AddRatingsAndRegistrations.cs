using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjekatWebKulturniDogadjaji.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingsAndRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventRegistrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Ratings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAt",
                table: "EventRegistrations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
