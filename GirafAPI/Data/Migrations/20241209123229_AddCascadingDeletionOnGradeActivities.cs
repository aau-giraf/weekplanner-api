using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GirafAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadingDeletionOnGradeActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Grades_GradeId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Grades_GradeId",
                table: "Activities",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Grades_GradeId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Grades_GradeId",
                table: "Activities",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");
        }
    }
}
