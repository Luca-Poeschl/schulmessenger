using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulmessenger.Persistence.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeId",
                schema: "schulmessenger",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StudentId",
                schema: "schulmessenger",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "schulmessenger",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                schema: "schulmessenger",
                table: "User");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                schema: "schulmessenger",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                schema: "schulmessenger",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
