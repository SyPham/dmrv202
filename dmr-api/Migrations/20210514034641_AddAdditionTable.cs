using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddAdditionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Additions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkerName = table.Column<string>(nullable: true),
                    LineID = table.Column<int>(nullable: false),
                    BPFCEstablishID = table.Column<int>(nullable: false),
                    ChemicalID = table.Column<int>(nullable: false),
                    Remark = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    DeletedBy = table.Column<int>(nullable: false),
                    ModifiedTime = table.Column<DateTime>(nullable: false),
                    DeletedTime = table.Column<DateTime>(nullable: false),
                    BuildingID = table.Column<int>(nullable: true),
                    IngredientID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Additions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Additions_BPFCEstablishes_BPFCEstablishID",
                        column: x => x.BPFCEstablishID,
                        principalTable: "BPFCEstablishes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Additions_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Additions_Ingredients_IngredientID",
                        column: x => x.IngredientID,
                        principalTable: "Ingredients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

          
            migrationBuilder.CreateIndex(
                name: "IX_Additions_BPFCEstablishID",
                table: "Additions",
                column: "BPFCEstablishID");

            migrationBuilder.CreateIndex(
                name: "IX_Additions_BuildingID",
                table: "Additions",
                column: "BuildingID");

            migrationBuilder.CreateIndex(
                name: "IX_Additions_IngredientID",
                table: "Additions",
                column: "IngredientID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Additions");
        }
    }
}
