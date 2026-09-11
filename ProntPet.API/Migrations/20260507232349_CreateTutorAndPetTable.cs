using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProntPet.Migrations
{
    /// <inheritdoc />
    public partial class CreateTutorAndPetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DB_TUTOR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CPF = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    PHONE = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PASSWORD = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    ADDRESS = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DB_TUTOR", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DB_PET",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_TUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SPECIES = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    BREED = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    AGE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    WEIGHT = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    SEX = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DB_PET", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DB_PET_DB_TUTOR_ID_TUTOR",
                        column: x => x.ID_TUTOR,
                        principalTable: "DB_TUTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DB_PET_ID_TUTOR",
                table: "DB_PET",
                column: "ID_TUTOR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB_PET");

            migrationBuilder.DropTable(
                name: "DB_TUTOR");
        }
    }
}
