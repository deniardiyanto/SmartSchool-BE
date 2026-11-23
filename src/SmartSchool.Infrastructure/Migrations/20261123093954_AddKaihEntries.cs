using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSchool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKaihEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kaih_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WakeUpEarly = table.Column<bool>(type: "boolean", nullable: false),
                    Worship = table.Column<bool>(type: "boolean", nullable: false),
                    Exercise = table.Column<bool>(type: "boolean", nullable: false),
                    HealthyEating = table.Column<bool>(type: "boolean", nullable: false),
                    Learning = table.Column<bool>(type: "boolean", nullable: false),
                    Socializing = table.Column<bool>(type: "boolean", nullable: false),
                    EarlySleep = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kaih_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kaih_entries_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kaih_entries_StudentId_EntryDate",
                table: "kaih_entries",
                columns: new[] { "StudentId", "EntryDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kaih_entries");
        }
    }
}
