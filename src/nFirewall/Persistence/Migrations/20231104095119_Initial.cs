using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nFirewall.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannedAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    Permanent = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannedAddresses_ExpireDate",
                table: "BannedAddresses",
                column: "ExpireDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedAddresses");
        }
    }
}
