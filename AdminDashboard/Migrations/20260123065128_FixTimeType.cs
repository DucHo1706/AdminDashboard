using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class FixTimeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiXe",
                columns: table => new
                {
                    LoaiXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenLoaiXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiXe", x => x.LoaiXeId);
                });

            migrationBuilder.CreateTable(
                name: "NhaXe",
                columns: table => new
                {
                    NhaXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenNhaXe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaXe", x => x.NhaXeId);
                });

            migrationBuilder.CreateTable(
                name: "Tram",
                columns: table => new
                {
                    IdTram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenTram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChiTram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Huyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Xa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tram", x => x.IdTram);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NhaXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_NguoiDung_NhaXe_NhaXeId",
                        column: x => x.NhaXeId,
                        principalTable: "NhaXe",
                        principalColumn: "NhaXeId");
                });

            migrationBuilder.CreateTable(
                name: "Xe",
                columns: table => new
                {
                    XeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BienSoXe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoaiXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NhaXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xe", x => x.XeId);
                    table.ForeignKey(
                        name: "FK_Xe_LoaiXe_LoaiXeId",
                        column: x => x.LoaiXeId,
                        principalTable: "LoaiXe",
                        principalColumn: "LoaiXeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Xe_NhaXe_NhaXeId",
                        column: x => x.NhaXeId,
                        principalTable: "NhaXe",
                        principalColumn: "NhaXeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoTrinh",
                columns: table => new
                {
                    LoTrinhId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TramDi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TramToi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GiaVeCoDinh = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoTrinh", x => x.LoTrinhId);
                    table.ForeignKey(
                        name: "FK_LoTrinh_Tram_TramDi",
                        column: x => x.TramDi,
                        principalTable: "Tram",
                        principalColumn: "IdTram",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoTrinh_Tram_TramToi",
                        column: x => x.TramToi,
                        principalTable: "Tram",
                        principalColumn: "IdTram",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_VaiTro_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VaiTro",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ghe",
                columns: table => new
                {
                    GheID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    XeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoGhe = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ghe", x => x.GheID);
                    table.ForeignKey(
                        name: "FK_Ghe_Xe_XeId",
                        column: x => x.XeId,
                        principalTable: "Xe",
                        principalColumn: "XeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenXe",
                columns: table => new
                {
                    ChuyenId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoTrinhId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    XeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TaiXeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayDi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioDi = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioDenDuKien = table.Column<TimeSpan>(type: "time", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenXe", x => x.ChuyenId);
                    table.ForeignKey(
                        name: "FK_ChuyenXe_LoTrinh_LoTrinhId",
                        column: x => x.LoTrinhId,
                        principalTable: "LoTrinh",
                        principalColumn: "LoTrinhId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChuyenXe_NguoiDung_TaiXeId",
                        column: x => x.TaiXeId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ChuyenXe_Xe_XeId",
                        column: x => x.XeId,
                        principalTable: "Xe",
                        principalColumn: "XeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenXeImage",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChuyenId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenXeImage", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ChuyenXeImage_ChuyenXe_ChuyenId",
                        column: x => x.ChuyenId,
                        principalTable: "ChuyenXe",
                        principalColumn: "ChuyenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    DonHangId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IDKhachHang = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ChuyenId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThoiGianHetHan = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.DonHangId);
                    table.ForeignKey(
                        name: "FK_DonHang_ChuyenXe_ChuyenId",
                        column: x => x.ChuyenId,
                        principalTable: "ChuyenXe",
                        principalColumn: "ChuyenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHang_NguoiDung_IDKhachHang",
                        column: x => x.IDKhachHang,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ve",
                columns: table => new
                {
                    VeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DonHangId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GheID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Gia = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ve", x => x.VeId);
                    table.ForeignKey(
                        name: "FK_Ve_DonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHang",
                        principalColumn: "DonHangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ve_Ghe_GheID",
                        column: x => x.GheID,
                        principalTable: "Ghe",
                        principalColumn: "GheID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "RoleId", "TenVaiTro" },
                values: new object[,]
                {
                    { "b9f3d6a1-5c8e-4a7d-9b2c-1e3f4a5d6c7b", "Admin" },
                    { "c8e2f1a0-4d9b-3a6c-8e1f-0d2e3b4a5c6d", "ChuNhaXe" },
                    { "d7a1e0b9-3c8f-2a5e-7d0b-9c1f2e3d4a5b", "NhanVien" },
                    { "e6c0d9a8-2b7e-1a4d-6c9a-8b0f1e2d3c4a", "TaiXe" },
                    { "f5b9c8a7-1a6d-0e3c-5b8a-7a9f0e1d2c3b", "KhachHang" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_LoTrinhId",
                table: "ChuyenXe",
                column: "LoTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_TaiXeId",
                table: "ChuyenXe",
                column: "TaiXeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_XeId",
                table: "ChuyenXe",
                column: "XeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXeImage_ChuyenId",
                table: "ChuyenXeImage",
                column: "ChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ChuyenId",
                table: "DonHang",
                column: "ChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_IDKhachHang",
                table: "DonHang",
                column: "IDKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_XeId",
                table: "Ghe",
                column: "XeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoTrinh_TramDi",
                table: "LoTrinh",
                column: "TramDi");

            migrationBuilder.CreateIndex(
                name: "IX_LoTrinh_TramToi",
                table: "LoTrinh",
                column: "TramToi");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_NhaXeId",
                table: "NguoiDung",
                column: "NhaXeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_DonHangId",
                table: "Ve",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_GheID",
                table: "Ve",
                column: "GheID");

            migrationBuilder.CreateIndex(
                name: "IX_Xe_LoaiXeId",
                table: "Xe",
                column: "LoaiXeId");

            migrationBuilder.CreateIndex(
                name: "IX_Xe_NhaXeId",
                table: "Xe",
                column: "NhaXeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuyenXeImage");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Ve");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "Ghe");

            migrationBuilder.DropTable(
                name: "ChuyenXe");

            migrationBuilder.DropTable(
                name: "LoTrinh");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Xe");

            migrationBuilder.DropTable(
                name: "Tram");

            migrationBuilder.DropTable(
                name: "LoaiXe");

            migrationBuilder.DropTable(
                name: "NhaXe");
        }
    }
}
