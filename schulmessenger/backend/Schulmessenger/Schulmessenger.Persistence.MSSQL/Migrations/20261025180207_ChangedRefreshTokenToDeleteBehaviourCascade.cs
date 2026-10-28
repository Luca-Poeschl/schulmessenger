using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRefreshTokenToDeleteBehaviourCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_User_UserId",
                schema: "schulmessenger",
                table: "RefreshToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_User_UserId",
                schema: "schulmessenger",
                table: "RefreshToken",
                column: "UserId",
                principalSchema: "schulmessenger",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_User_UserId",
                schema: "schulmessenger",
                table: "RefreshToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_User_UserId",
                schema: "schulmessenger",
                table: "RefreshToken",
                column: "UserId",
                principalSchema: "schulmessenger",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
