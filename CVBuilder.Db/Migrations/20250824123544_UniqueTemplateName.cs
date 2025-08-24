﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVBuilder.Db.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTemplateName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Templates_Name",
                table: "Templates",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_Name",
                table: "Templates");
        }
    }
}
