using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class AlteraAgeParaBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AGE",
                table: "DB_PET");

            migrationBuilder.AddColumn<DateTime>(
                name: "BIRTH_DATE",
                table: "DB_PET",
                type: "TIMESTAMP(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BIRTH_DATE",
                table: "DB_PET");

            migrationBuilder.AddColumn<int>(
                name: "AGE",
                table: "DB_PET",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }
    }
}
