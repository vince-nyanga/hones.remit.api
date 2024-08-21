using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hones.Remit.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateValidationSucceeded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateValidationSucceededUtc",
                schema: "remit",
                table: "order_states",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateValidationSucceededUtc",
                schema: "remit",
                table: "order_states");
        }
    }
}
