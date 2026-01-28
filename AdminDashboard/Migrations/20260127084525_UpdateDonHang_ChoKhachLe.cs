using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonHang_ChoKhachLe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IDKhachHang",
                table: "DonHang",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "EmailNguoiDat",
                table: "DonHang",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "DonHang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HoTenNguoiDat",
                table: "DonHang",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SdtNguoiDat",
                table: "DonHang",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailNguoiDat",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "HoTenNguoiDat",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "SdtNguoiDat",
                table: "DonHang");

            migrationBuilder.AlterColumn<string>(
                name: "IDKhachHang",
                table: "DonHang",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
