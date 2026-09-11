using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class CreateClinicAndConsultationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CNPJ = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: false),
                    ADDRESS = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_MEDICAL_RECORD = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_CLINIC = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CONSULTATION_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    SYMPTOMS = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DIAGNOSIS = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    OBSERVATIONS = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Consultations_Clinics_ID_CLINIC",
                        column: x => x.ID_CLINIC,
                        principalTable: "Clinics",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultations_DB_MEDICAL_RECORD_ID_MEDICAL_RECORD",
                        column: x => x.ID_MEDICAL_RECORD,
                        principalTable: "DB_MEDICAL_RECORD",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ID_CLINIC",
                table: "Consultations",
                column: "ID_CLINIC");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ID_MEDICAL_RECORD",
                table: "Consultations",
                column: "ID_MEDICAL_RECORD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "Clinics");
        }
    }
}
