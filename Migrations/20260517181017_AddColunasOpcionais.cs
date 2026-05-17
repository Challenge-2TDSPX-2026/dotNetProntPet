using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class AddColunasOpcionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD");

            migrationBuilder.AlterColumn<decimal>(
                name: "WEIGHT",
                table: "DB_PET",
                type: "NUMBER(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "NAME",
                table: "DB_PET",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "BREED",
                table: "DB_PET",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CHRONIC_DISEASES",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ALLERGIES",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD",
                column: "MICROCHIP_CODE",
                unique: true,
                filter: "\"MICROCHIP_CODE\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD");

            migrationBuilder.AlterColumn<decimal>(
                name: "WEIGHT",
                table: "DB_PET",
                type: "NUMBER(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NAME",
                table: "DB_PET",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BREED",
                table: "DB_PET",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CHRONIC_DISEASES",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ALLERGIES",
                table: "DB_MEDICAL_RECORD",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD",
                column: "MICROCHIP_CODE",
                unique: true);
        }
    }
}
