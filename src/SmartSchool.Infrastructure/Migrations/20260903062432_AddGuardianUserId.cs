using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSchool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuardianUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "guardians",
                type: "uuid",
                nullable: true);

            
            migrationBuilder.CreateIndex(
                name: "IX_guardians_UserId",
                table: "guardians",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_guardians_users_UserId",
                table: "guardians",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guardians_users_UserId",
                table: "guardians");

            migrationBuilder.DropIndex(
                name: "IX_guardians_UserId",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "guardians");

        }
    }
}
