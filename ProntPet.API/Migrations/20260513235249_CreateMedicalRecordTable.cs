using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class CreateMedicalRecordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DB_MEDICAL_RECORD",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BLOOD_TYPE = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    ALLERGIES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    CHRONIC_DISEASES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    IS_CASTRATED = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    MICROCHIP_CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    LAST_UPDATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DB_MEDICAL_RECORD", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DB_MEDICAL_RECORD_DB_PET_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "DB_PET",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_ID_PET",
                table: "DB_MEDICAL_RECORD",
                column: "ID_PET");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_MEDICAL_RECORD");
        }
    }
}
