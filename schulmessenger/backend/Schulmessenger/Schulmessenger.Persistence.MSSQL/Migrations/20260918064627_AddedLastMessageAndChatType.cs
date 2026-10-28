using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedLastMessageAndChatType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                schema: "schulmessenger",
                table: "Message",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ChatType",
                schema: "schulmessenger",
                table: "Chat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastMessage",
                schema: "schulmessenger",
                table: "Chat",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageSentAt",
                schema: "schulmessenger",
                table: "Chat",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentAt",
                schema: "schulmessenger",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "ChatType",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "LastMessage",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "LastMessageSentAt",
                schema: "schulmessenger",
                table: "Chat");
        }
    }
}
