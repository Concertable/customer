using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertable.Customer.Concert.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "concert");

            migrationBuilder.CreateTable(
                name: "Concerts",
                schema: "concert",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    AvailableTickets = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period_Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period_End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    ArtistName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayeeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concerts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Concerts",
                schema: "concert");
        }
    }
}
