using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserChatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatUser",
                schema: "schulmessenger");

            migrationBuilder.CreateTable(
                name: "UserChat",
                schema: "schulmessenger",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChat", x => new { x.UserId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_UserChat_Chat_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "schulmessenger",
                        principalTable: "Chat",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChat_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "schulmessenger",
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserChat_ChatId",
                schema: "schulmessenger",
                table: "UserChat",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChat",
                schema: "schulmessenger");

            migrationBuilder.CreateTable(
                name: "ChatUser",
                schema: "schulmessenger",
                columns: table => new
                {
                    ChatsChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUser", x => new { x.ChatsChatId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_ChatUser_Chat_ChatsChatId",
                        column: x => x.ChatsChatId,
                        principalSchema: "schulmessenger",
                        principalTable: "Chat",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatUser_User_UsersUserId",
                        column: x => x.UsersUserId,
                        principalSchema: "schulmessenger",
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatUser_UsersUserId",
                schema: "schulmessenger",
                table: "ChatUser",
                column: "UsersUserId");
        }
    }
}
