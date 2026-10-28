using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangedLastMessageToNavigationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_Chat_ChatId",
                schema: "schulmessenger",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "LastMessage",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "LastMessageSentAt",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.AddColumn<Guid>(
                name: "LastMessageId",
                schema: "schulmessenger",
                table: "Chat",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_LastMessageId",
                schema: "schulmessenger",
                table: "Chat",
                column: "LastMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LastMessageId",
                schema: "schulmessenger",
                table: "Chat",
                column: "LastMessageId",
                principalSchema: "schulmessenger",
                principalTable: "Message",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Chat_ChatId",
                schema: "schulmessenger",
                table: "Message",
                column: "ChatId",
                principalSchema: "schulmessenger",
                principalTable: "Chat",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LastMessageId",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Chat_ChatId",
                schema: "schulmessenger",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_LastMessageId",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                schema: "schulmessenger",
                table: "Chat");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Chat_ChatId",
                schema: "schulmessenger",
                table: "Message",
                column: "ChatId",
                principalSchema: "schulmessenger",
                principalTable: "Chat",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
