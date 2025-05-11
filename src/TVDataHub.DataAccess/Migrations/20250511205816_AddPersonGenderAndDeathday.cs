using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVDataHub.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonGenderAndDeathday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Deathday",
                table: "Persons",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Persons",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deathday",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Persons");
        }
    }
}
