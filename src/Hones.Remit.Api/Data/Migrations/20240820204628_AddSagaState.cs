using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hones.Remit.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_states",
                schema: "remit",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateCreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DatePaidUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCancelledUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateReadyForCollection = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCollectedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateExpiredUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_states", x => x.OrderId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_states",
                schema: "remit");
        }
    }
}
