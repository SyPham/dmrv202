using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateAdditionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Additions_Ingredients_IngredientID",
                table: "Additions");

            migrationBuilder.DropIndex(
                name: "IX_Additions_IngredientID",
                table: "Additions");

            migrationBuilder.DropColumn(
                name: "IngredientID",
                table: "Additions");

            migrationBuilder.CreateIndex(
                name: "IX_Additions_ChemicalID",
                table: "Additions",
                column: "ChemicalID");

            migrationBuilder.AddForeignKey(
                name: "FK_Additions_Ingredients_ChemicalID",
                table: "Additions",
                column: "ChemicalID",
                principalTable: "Ingredients",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Additions_Ingredients_ChemicalID",
                table: "Additions");

            migrationBuilder.DropIndex(
                name: "IX_Additions_ChemicalID",
                table: "Additions");

            migrationBuilder.AddColumn<int>(
                name: "IngredientID",
                table: "Additions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Additions_IngredientID",
                table: "Additions",
                column: "IngredientID");

            migrationBuilder.AddForeignKey(
                name: "FK_Additions_Ingredients_IngredientID",
                table: "Additions",
                column: "IngredientID",
                principalTable: "Ingredients",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
