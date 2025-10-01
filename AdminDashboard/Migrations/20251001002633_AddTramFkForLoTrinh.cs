using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddTramFkForLoTrinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiXe",
                columns: table => new
                {
                    LoaiXeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenLoaiXe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiXe", x => x.LoaiXeId);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenMenu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ViTri = table.Column<int>(type: "int", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Tram",
                columns: table => new
                {
                    IdTram = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenTram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChiTram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tram", x => x.IdTram);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Xe",
                columns: table => new
                {
                    XeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BienSoXe = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiXeId = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xe", x => x.XeId);
                    table.ForeignKey(
                        name: "FK_Xe_LoaiXe_LoaiXeId",
                        column: x => x.LoaiXeId,
                        principalTable: "LoaiXe",
                        principalColumn: "LoaiXeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaiViet",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AdminUserId = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiViet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaiViet_NguoiDung_AdminId",
                        column: x => x.AdminId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_BaiViet_NguoiDung_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    IDKhachHang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChiMail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.IDKhachHang);
                    table.ForeignKey(
                        name: "FK_KhachHang_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "LoTrinh",
                columns: table => new
                {
                    LoTrinhId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TramDi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TramToi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GiaVeCoDinh = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
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
                    UserId = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(10)", nullable: false)
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
                    GheID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    XeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoGhe = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    XeId1 = table.Column<string>(type: "nvarchar(10)", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_Ghe_Xe_XeId1",
                        column: x => x.XeId1,
                        principalTable: "Xe",
                        principalColumn: "XeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenXe",
                columns: table => new
                {
                    ChuyenId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LoTrinhId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    XeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayDi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioDi = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioDenDuKien = table.Column<TimeSpan>(type: "time", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                        name: "FK_ChuyenXe_Xe_XeId",
                        column: x => x.XeId,
                        principalTable: "Xe",
                        principalColumn: "XeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    DonHangId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IDKhachHang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChuyenId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KhachHangIDKhachHang = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    ChuyenXeChuyenId = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.DonHangId);
                    table.ForeignKey(
                        name: "FK_DonHang_ChuyenXe_ChuyenId",
                        column: x => x.ChuyenId,
                        principalTable: "ChuyenXe",
                        principalColumn: "ChuyenId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonHang_ChuyenXe_ChuyenXeChuyenId",
                        column: x => x.ChuyenXeChuyenId,
                        principalTable: "ChuyenXe",
                        principalColumn: "ChuyenId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonHang_KhachHang_IDKhachHang",
                        column: x => x.IDKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "IDKhachHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonHang_KhachHang_KhachHangIDKhachHang",
                        column: x => x.KhachHangIDKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "IDKhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ve",
                columns: table => new
                {
                    VeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DonHangId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GheID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Gia = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DonHangId1 = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    GheID1 = table.Column<string>(type: "nvarchar(10)", nullable: false)
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
                        name: "FK_Ve_DonHang_DonHangId1",
                        column: x => x.DonHangId1,
                        principalTable: "DonHang",
                        principalColumn: "DonHangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ve_Ghe_GheID",
                        column: x => x.GheID,
                        principalTable: "Ghe",
                        principalColumn: "GheID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ve_Ghe_GheID1",
                        column: x => x.GheID1,
                        principalTable: "Ghe",
                        principalColumn: "GheID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiViet_AdminId",
                table: "BaiViet",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiViet_AdminUserId",
                table: "BaiViet",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_LoTrinhId",
                table: "ChuyenXe",
                column: "LoTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_NgayDi",
                table: "ChuyenXe",
                column: "NgayDi");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenXe_XeId",
                table: "ChuyenXe",
                column: "XeId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ChuyenId",
                table: "DonHang",
                column: "ChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ChuyenXeChuyenId",
                table: "DonHang",
                column: "ChuyenXeChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_IDKhachHang",
                table: "DonHang",
                column: "IDKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_KhachHangIDKhachHang",
                table: "DonHang",
                column: "KhachHangIDKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_XeId_SoGhe",
                table: "Ghe",
                columns: new[] { "XeId", "SoGhe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_XeId1",
                table: "Ghe",
                column: "XeId1");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_DiaChiMail",
                table: "KhachHang",
                column: "DiaChiMail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_UserId",
                table: "KhachHang",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoTrinh_TramDi_TramToi",
                table: "LoTrinh",
                columns: new[] { "TramDi", "TramToi" });

            migrationBuilder.CreateIndex(
                name: "IX_LoTrinh_TramToi",
                table: "LoTrinh",
                column: "TramToi");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_TenVaiTro",
                table: "VaiTro",
                column: "TenVaiTro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ve_DonHangId_GheID",
                table: "Ve",
                columns: new[] { "DonHangId", "GheID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ve_DonHangId1",
                table: "Ve",
                column: "DonHangId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_GheID",
                table: "Ve",
                column: "GheID");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_GheID1",
                table: "Ve",
                column: "GheID1");

            migrationBuilder.CreateIndex(
                name: "IX_Xe_BienSoXe",
                table: "Xe",
                column: "BienSoXe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Xe_LoaiXeId",
                table: "Xe",
                column: "LoaiXeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiViet");

            migrationBuilder.DropTable(
                name: "Menu");

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
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "LoTrinh");

            migrationBuilder.DropTable(
                name: "Xe");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Tram");

            migrationBuilder.DropTable(
                name: "LoaiXe");
        }
    }
}
