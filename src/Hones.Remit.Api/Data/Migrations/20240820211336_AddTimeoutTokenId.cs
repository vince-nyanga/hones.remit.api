using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hones.Remit.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeoutTokenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTimeoutTokenId",
                schema: "remit",
                table: "order_states",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTimeoutTokenId",
                schema: "remit",
                table: "order_states");
        }
    }
}
