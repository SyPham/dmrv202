using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddWorkerIDAdditionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Additions_Buildings_BuildingID",
                table: "Additions");

            migrationBuilder.DropIndex(
                name: "IX_Additions_BuildingID",
                table: "Additions");

            migrationBuilder.DropColumn(
                name: "BuildingID",
                table: "Additions");

            migrationBuilder.DropColumn(
                name: "WorkerName",
                table: "Additions");

            migrationBuilder.AddColumn<string>(
                name: "WorkerID",
                table: "Additions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Additions_LineID",
                table: "Additions",
                column: "LineID");

            migrationBuilder.AddForeignKey(
                name: "FK_Additions_Buildings_LineID",
                table: "Additions",
                column: "LineID",
                principalTable: "Buildings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Additions_Buildings_LineID",
                table: "Additions");

            migrationBuilder.DropIndex(
                name: "IX_Additions_LineID",
                table: "Additions");

            migrationBuilder.DropColumn(
                name: "WorkerID",
                table: "Additions");

            migrationBuilder.AddColumn<int>(
                name: "BuildingID",
                table: "Additions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerName",
                table: "Additions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Additions_BuildingID",
                table: "Additions",
                column: "BuildingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Additions_Buildings_BuildingID",
                table: "Additions",
                column: "BuildingID",
                principalTable: "Buildings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
