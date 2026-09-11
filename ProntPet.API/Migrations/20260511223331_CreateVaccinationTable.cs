using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class CreateVaccinationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DB_VACCINATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VACCINE_NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    APPLICATION_DATE = table.Column<string>(type: "NVARCHAR2(10)", nullable: false),
                    EXPIRATION_DATE = table.Column<string>(type: "NVARCHAR2(10)", nullable: false),
                    LOT = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DB_VACCINATION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DB_VACCINATION_DB_PET_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "DB_PET",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DB_VACCINATION_ID_PET",
                table: "DB_VACCINATION",
                column: "ID_PET");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_VACCINATION");
        }
    }
}
