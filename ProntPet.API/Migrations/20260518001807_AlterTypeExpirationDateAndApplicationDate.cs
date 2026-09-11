using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class AlterTypeExpirationDateAndApplicationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EXPIRATION_DATE",
                table: "DB_VACCINATION",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(10)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "APPLICATION_DATE",
                table: "DB_VACCINATION",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EXPIRATION_DATE",
                table: "DB_VACCINATION",
                type: "NVARCHAR2(10)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AlterColumn<string>(
                name: "APPLICATION_DATE",
                table: "DB_VACCINATION",
                type: "NVARCHAR2(10)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");
        }
    }
}
