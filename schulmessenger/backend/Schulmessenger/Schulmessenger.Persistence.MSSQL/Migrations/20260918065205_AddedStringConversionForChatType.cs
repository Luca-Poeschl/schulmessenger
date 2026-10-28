using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedStringConversionForChatType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChatType",
                schema: "schulmessenger",
                table: "Chat",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ChatType",
                schema: "schulmessenger",
                table: "Chat",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
