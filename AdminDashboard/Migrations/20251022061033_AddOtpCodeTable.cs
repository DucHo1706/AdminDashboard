<<<<<<< HEAD
using System;
=======
﻿using System;
>>>>>>> master
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpCodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCode", x => x.Id);
                });
<<<<<<< HEAD

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
=======
>>>>>>> master
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                table: "ChuyenXe");

            migrationBuilder.DropTable(
                name: "OtpCode");

            migrationBuilder.DropIndex(
                name: "IX_ChuyenXe_TaiXeId",
                table: "ChuyenXe");
=======
            migrationBuilder.DropTable(
                name: "OtpCode");
>>>>>>> master
        }
    }
}
