using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Clinics_ID_CLINIC",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_DB_MEDICAL_RECORD_ID_PET",
                table: "DB_MEDICAL_RECORD");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clinics",
                table: "Clinics");

            migrationBuilder.RenameTable(
                name: "Clinics",
                newName: "DB_CLINIC");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DB_CLINIC",
                table: "DB_CLINIC",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_DB_TUTOR_CPF",
                table: "DB_TUTOR",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_TUTOR_EMAIL",
                table: "DB_TUTOR",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_TUTOR_PHONE",
                table: "DB_TUTOR",
                column: "PHONE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_ID_PET",
                table: "DB_MEDICAL_RECORD",
                column: "ID_PET",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD",
                column: "MICROCHIP_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DB_CLINIC_CNPJ",
                table: "DB_CLINIC",
                column: "CNPJ",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_DB_CLINIC_ID_CLINIC",
                table: "Consultations",
                column: "ID_CLINIC",
                principalTable: "DB_CLINIC",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_DB_CLINIC_ID_CLINIC",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_DB_TUTOR_CPF",
                table: "DB_TUTOR");

            migrationBuilder.DropIndex(
                name: "IX_DB_TUTOR_EMAIL",
                table: "DB_TUTOR");

            migrationBuilder.DropIndex(
                name: "IX_DB_TUTOR_PHONE",
                table: "DB_TUTOR");

            migrationBuilder.DropIndex(
                name: "IX_DB_MEDICAL_RECORD_ID_PET",
                table: "DB_MEDICAL_RECORD");

            migrationBuilder.DropIndex(
                name: "IX_DB_MEDICAL_RECORD_MICROCHIP_CODE",
                table: "DB_MEDICAL_RECORD");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DB_CLINIC",
                table: "DB_CLINIC");

            migrationBuilder.DropIndex(
                name: "IX_DB_CLINIC_CNPJ",
                table: "DB_CLINIC");

            migrationBuilder.RenameTable(
                name: "DB_CLINIC",
                newName: "Clinics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clinics",
                table: "Clinics",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_DB_MEDICAL_RECORD_ID_PET",
                table: "DB_MEDICAL_RECORD",
                column: "ID_PET");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Clinics_ID_CLINIC",
                table: "Consultations",
                column: "ID_CLINIC",
                principalTable: "Clinics",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
