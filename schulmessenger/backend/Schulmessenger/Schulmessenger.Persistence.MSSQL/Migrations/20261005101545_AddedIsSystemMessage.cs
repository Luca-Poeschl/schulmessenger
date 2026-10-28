using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsSystemMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                schema: "schulmessenger",
                table: "Student",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                schema: "schulmessenger",
                table: "Message",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemMessage",
                schema: "schulmessenger",
                table: "Message",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Class",
                schema: "schulmessenger",
                columns: table => new
                {
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.ClassId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassId",
                schema: "schulmessenger",
                table: "Student",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Class_ClassId",
                schema: "schulmessenger",
                table: "Student",
                column: "ClassId",
                principalSchema: "schulmessenger",
                principalTable: "Class",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Student_Class_ClassId",
                schema: "schulmessenger",
                table: "Student");

            migrationBuilder.DropTable(
                name: "Class",
                schema: "schulmessenger");

            migrationBuilder.DropIndex(
                name: "IX_Student_ClassId",
                schema: "schulmessenger",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "ClassId",
                schema: "schulmessenger",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "IsSystemMessage",
                schema: "schulmessenger",
                table: "Message");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                schema: "schulmessenger",
                table: "Message",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
