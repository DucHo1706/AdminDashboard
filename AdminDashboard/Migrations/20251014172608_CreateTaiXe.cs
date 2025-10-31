using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class CreateTaiXe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "TaiXe",
            //    columns: table => new
            //    {
            //        TaiXeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
            //        AdminId = table.Column<string>(type: "nvarchar(255)", nullable: false),
            //        BangLaiXe = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        NgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TaiXe", x => x.TaiXeId);
            //        table.ForeignKey(
            //            name: "FK_TaiXe_NguoiDung_AdminId",
            //            column: x => x.AdminId,
            //            principalTable: "NguoiDung",
            //            principalColumn: "UserId");
            //        table.ForeignKey(
            //            name: "FK_TaiXe_NguoiDung_UserId",
            //            column: x => x.UserId,
            //            principalTable: "NguoiDung",
            //            principalColumn: "UserId");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_ChuyenXe_TaiXeId",
            //    table: "ChuyenXe",
            //    column: "TaiXeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TaiXe_AdminId",
            //    table: "TaiXe",
            //    column: "AdminId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_TaiXe_UserId",
            //    table: "TaiXe",
            //    column: "UserId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_ChuyenXe_NguoiDung_TaiXeId",
            //    table: "ChuyenXe",
            //    column: "TaiXeId",
            //    principalTable: "NguoiDung",
            //    principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.DropTable(
                name: "TaiXe");

            migrationBuilder.DropIndex(
                name: "IX_ChuyenXe_TaiXeId",
                table: "ChuyenXe");
        }
    }
}
