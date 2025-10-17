using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class Update_TaiXe_UserIdLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiXe_NguoiDung_AdminId",
                table: "TaiXe");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiXe_NguoiDung_UserId",
                table: "TaiXe");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TaiXe",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "TaiXe",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BangLaiXe",
                table: "TaiXe",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "TaiXe",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<int>(
                name: "TaiXeId",
                table: "TaiXe",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXe_NguoiDung_AdminId",
                table: "TaiXe",
                column: "AdminId",
                principalTable: "NguoiDung",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXe_NguoiDung_UserId",
                table: "TaiXe",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiXe_NguoiDung_AdminId",
                table: "TaiXe");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiXe_NguoiDung_UserId",
                table: "TaiXe");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TaiXe",
                type: "nvarchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "TaiXe",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "BangLaiXe",
                table: "TaiXe",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "TaiXe",
                type: "nvarchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "TaiXeId",
                table: "TaiXe",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXe_NguoiDung_AdminId",
                table: "TaiXe",
                column: "AdminId",
                principalTable: "NguoiDung",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiXe_NguoiDung_UserId",
                table: "TaiXe",
                column: "UserId",
                principalTable: "NguoiDung",
                principalColumn: "UserId");
        }
    }
}
