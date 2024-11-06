using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GirafAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class SingleCitizenOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitizenOrganization");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Citizens",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Citizens_OrganizationId",
                table: "Citizens",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citizens_Organizations_OrganizationId",
                table: "Citizens",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citizens_Organizations_OrganizationId",
                table: "Citizens");

            migrationBuilder.DropIndex(
                name: "IX_Citizens_OrganizationId",
                table: "Citizens");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Citizens");

            migrationBuilder.CreateTable(
                name: "CitizenOrganization",
                columns: table => new
                {
                    CitizensId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenOrganization", x => new { x.CitizensId, x.OrganizationsId });
                    table.ForeignKey(
                        name: "FK_CitizenOrganization_Citizens_CitizensId",
                        column: x => x.CitizensId,
                        principalTable: "Citizens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CitizenOrganization_Organizations_OrganizationsId",
                        column: x => x.OrganizationsId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitizenOrganization_OrganizationsId",
                table: "CitizenOrganization",
                column: "OrganizationsId");
        }
    }
}
