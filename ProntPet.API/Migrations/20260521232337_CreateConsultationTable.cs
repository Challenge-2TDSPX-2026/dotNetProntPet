using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class CreateConsultationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_DB_CLINIC_ID_CLINIC",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_DB_MEDICAL_RECORD_ID_MEDICAL_RECORD",
                table: "Consultations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consultations",
                table: "Consultations");

            migrationBuilder.RenameTable(
                name: "Consultations",
                newName: "DB_CONSULTATION");

            migrationBuilder.RenameIndex(
                name: "IX_Consultations_ID_MEDICAL_RECORD",
                table: "DB_CONSULTATION",
                newName: "IX_DB_CONSULTATION_ID_MEDICAL_RECORD");

            migrationBuilder.RenameIndex(
                name: "IX_Consultations_ID_CLINIC",
                table: "DB_CONSULTATION",
                newName: "IX_DB_CONSULTATION_ID_CLINIC");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DB_CONSULTATION",
                table: "DB_CONSULTATION",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DB_CONSULTATION_DB_CLINIC_ID_CLINIC",
                table: "DB_CONSULTATION",
                column: "ID_CLINIC",
                principalTable: "DB_CLINIC",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DB_CONSULTATION_DB_MEDICAL_RECORD_ID_MEDICAL_RECORD",
                table: "DB_CONSULTATION",
                column: "ID_MEDICAL_RECORD",
                principalTable: "DB_MEDICAL_RECORD",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DB_CONSULTATION_DB_CLINIC_ID_CLINIC",
                table: "DB_CONSULTATION");

            migrationBuilder.DropForeignKey(
                name: "FK_DB_CONSULTATION_DB_MEDICAL_RECORD_ID_MEDICAL_RECORD",
                table: "DB_CONSULTATION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DB_CONSULTATION",
                table: "DB_CONSULTATION");

            migrationBuilder.RenameTable(
                name: "DB_CONSULTATION",
                newName: "Consultations");

            migrationBuilder.RenameIndex(
                name: "IX_DB_CONSULTATION_ID_MEDICAL_RECORD",
                table: "Consultations",
                newName: "IX_Consultations_ID_MEDICAL_RECORD");

            migrationBuilder.RenameIndex(
                name: "IX_DB_CONSULTATION_ID_CLINIC",
                table: "Consultations",
                newName: "IX_Consultations_ID_CLINIC");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consultations",
                table: "Consultations",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_DB_CLINIC_ID_CLINIC",
                table: "Consultations",
                column: "ID_CLINIC",
                principalTable: "DB_CLINIC",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_DB_MEDICAL_RECORD_ID_MEDICAL_RECORD",
                table: "Consultations",
                column: "ID_MEDICAL_RECORD",
                principalTable: "DB_MEDICAL_RECORD",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
