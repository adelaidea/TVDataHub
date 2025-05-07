using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TVDataHub.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Show",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CastMember",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>( nullable: false),
                    Birthday = table.Column<DateOnly>( nullable: false),
                    ShowId = table.Column<int>( nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CastMember_Show_ShowId",
                        column: x => x.ShowId,
                        principalTable: "Show",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CastMember_ShowId",
                table: "CastMember",
                column: "ShowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CastMember");

            migrationBuilder.DropTable(
                name: "Show");
        }
    }
}
