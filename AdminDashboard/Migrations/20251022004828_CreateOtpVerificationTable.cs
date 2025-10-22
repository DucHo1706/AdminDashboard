using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class CreateOtpVerificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpVerification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpVerification_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerification_UserId",
                table: "OtpVerification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpVerification");
        }
    }
}
