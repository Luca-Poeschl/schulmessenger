using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangedToDateTimeOffsetAndAddedCreatedAtToChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "JoinedAt",
                schema: "schulmessenger",
                table: "UserChat",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SentAt",
                schema: "schulmessenger",
                table: "Message",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "schulmessenger",
                table: "Chat",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "schulmessenger",
                table: "Chat");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                schema: "schulmessenger",
                table: "UserChat",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                schema: "schulmessenger",
                table: "Message",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
