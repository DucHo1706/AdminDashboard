using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddThoiGianHetHanToDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianHetHan",
                table: "DonHang",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_TaiXeId",
                table: "ChuyenXe",
                column: "TaiXeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe",
                column: "TaiXeId",
                principalTable: "NguoiDung",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.DropIndex(
                name: "IX_ChuyenXe_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.DropColumn(
                name: "ThoiGianHetHan",
                table: "DonHang");
        }
    }
}
