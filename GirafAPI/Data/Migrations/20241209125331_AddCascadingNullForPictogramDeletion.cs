using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GirafAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadingNullForPictogramDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Pictograms_PictogramId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Pictograms_PictogramId",
                table: "Activities",
                column: "PictogramId",
                principalTable: "Pictograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Pictograms_PictogramId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Pictograms_PictogramId",
                table: "Activities",
                column: "PictogramId",
                principalTable: "Pictograms",
                principalColumn: "Id");
        }
    }
}
