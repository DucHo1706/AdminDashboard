using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Relation_ChuyenXe_NhanVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuyenXe_NhanVien_TaiXeId",
                table: "ChuyenXe",
                column: "TaiXeId",
                principalTable: "NhanVien",
                principalColumn: "NhanVienId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenXe_NhanVien_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe",
                column: "TaiXeId",
                principalTable: "NguoiDung",
                principalColumn: "UserId");
        }
    }
}
