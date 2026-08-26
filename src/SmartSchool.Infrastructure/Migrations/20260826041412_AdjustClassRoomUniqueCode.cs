using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSchool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdjustClassRoomUniqueCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_class_rooms_Code",
                table: "class_rooms");

            migrationBuilder.CreateIndex(
                name: "IX_class_rooms_Code_AcademicYear",
                table: "class_rooms",
                columns: new[] { "Code", "AcademicYear" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_class_rooms_Code_AcademicYear",
                table: "class_rooms");

            migrationBuilder.CreateIndex(
                name: "IX_class_rooms_Code",
                table: "class_rooms",
                column: "Code",
                unique: true);
        }
    }
}
