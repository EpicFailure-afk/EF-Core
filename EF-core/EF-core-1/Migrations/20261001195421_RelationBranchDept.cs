using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_core_1.Migrations
{
    /// <inheritdoc />
    public partial class RelationBranchDept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "Department",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Department_BranchID",
                table: "Department",
                column: "BranchID");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Branch_BranchID",
                table: "Department",
                column: "BranchID",
                principalTable: "Branch",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Branch_BranchID",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_BranchID",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "Department");
        }
    }
}
