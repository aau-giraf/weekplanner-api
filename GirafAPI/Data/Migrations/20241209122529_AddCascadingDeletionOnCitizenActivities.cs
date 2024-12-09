using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GirafAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadingDeletionOnCitizenActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Citizens_CitizenId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Citizens_CitizenId",
                table: "Activities",
                column: "CitizenId",
                principalTable: "Citizens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Citizens_CitizenId",
                table: "Activities");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Citizens_CitizenId",
                table: "Activities",
                column: "CitizenId",
                principalTable: "Citizens",
                principalColumn: "Id");
        }
    }
}
