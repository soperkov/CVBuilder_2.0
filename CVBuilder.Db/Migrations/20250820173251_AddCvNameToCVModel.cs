using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVBuilder.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddCvNameToCVModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVName",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVName",
                table: "CVs");
        }
    }
}
