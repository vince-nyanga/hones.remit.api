using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hones.Remit.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "remit");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "remit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateCreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateExpiredUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DatePaidUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCancelledUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCollectedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SenderEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_PublicId",
                schema: "remit",
                table: "orders",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders",
                schema: "remit");
        }
    }
}
