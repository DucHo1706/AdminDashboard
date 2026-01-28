using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public class BanVeService : IBanVeService
    {
        private readonly Db27524Context _context;

        public BanVeService(Db27524Context context)
        {
            _context = context;
        }

        public async Task<List<ChuyenXe>> GetChuyenXeBanVeAsync(string nhaXeId, DateTime ngayDi)
        {
            return await _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Where(c => c.Xe.NhaXeId == nhaXeId
                         && c.NgayDi.Date == ngayDi.Date
                         && c.TrangThai != TrangThaiChuyenXe.DaHuy
                         && c.TrangThai != TrangThaiChuyenXe.ChoDuyet)
                .OrderBy(c => c.GioDi)
                .ToListAsync();
        }

        public async Task<SoDoGheDTO> GetSoDoGheAsync(string chuyenId, string nhaXeId)
        {
            var cx = await _context.ChuyenXe
                .Include(c => c.Xe)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId);

            if (cx == null || cx.Xe.NhaXeId != nhaXeId) return null;

            var veDaBan = await _context.Ve
               .Include(v => v.DonHang)
               .Include(v => v.Ghe)
               .Where(v => v.DonHang.ChuyenId == chuyenId && v.DonHang.TrangThaiThanhToan != "Da huy")
               .Select(v => new VeBanInfo // Dùng class DTO cụ thể hoặc anonymous type
               {
                   // QUAN TRỌNG: Đây là tên thuộc tính sẽ dùng bên View
                   SoGhe = v.Ghe.SoGhe,
                   TenKhach = v.DonHang.HoTenNguoiDat ?? "Khách Online",
                   Sdt = v.DonHang.SdtNguoiDat ?? "---",
                   TrangThai = v.DonHang.TrangThaiThanhToan
               })
               .ToListAsync();

            // Lưu ý: VeDaBan trong SoDoGheDTO cần khai báo là List<VeBanInfo> hoặc dynamic
            return new SoDoGheDTO { ChuyenXe = cx, VeDaBan = veDaBan };
        }

        public async Task<KetQuaBanVe> DatVeTaiQuayAsync(DatVeTaiQuayRequest req, string nhaXeId, string tenNhanVien)
        {
            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(c => c.ChuyenId == req.ChuyenId);

            if (chuyenXe == null) return new KetQuaBanVe { Success = false, Message = "Chuyến xe không tồn tại." };

            var gheDb = await _context.Ghe
                .FirstOrDefaultAsync(g => g.XeId == chuyenXe.XeId && g.SoGhe == req.SoGhe);

            if (gheDb == null)
            {
                gheDb = new Ghe
                {
                    GheID = Guid.NewGuid().ToString(),
                    SoGhe = req.SoGhe,
                    XeId = chuyenXe.XeId,
                    TrangThai = "Trong"
                };
                _context.Ghe.Add(gheDb);
                await _context.SaveChangesAsync();
            }

            var gheDaDat = await _context.Ve.AnyAsync(v =>
                v.DonHang.ChuyenId == req.ChuyenId &&
                v.GheID == gheDb.GheID &&
                v.DonHang.TrangThaiThanhToan != "Da huy");

            if (gheDaDat)
            {
                return new KetQuaBanVe { Success = false, Message = $"Ghế {req.SoGhe} đã có người đặt." };
            }

            try
            {
                var donHang = new DonHang
                {
                    DonHangId = Guid.NewGuid().ToString("N"),
                    ChuyenId = req.ChuyenId,
                    IDKhachHang = null,
                    NgayDat = DateTime.Now,
                    TongTien = req.GiaVe,
                    TrangThaiThanhToan = req.DaThanhToan ? "Da thanh toan" : "Cho thanh toan",
                    HoTenNguoiDat = req.HoTen,
                    SdtNguoiDat = req.SoDienThoai,
                    GhiChu = req.GhiChu ?? $"Bán tại quầy bởi {tenNhanVien}"
                };

                var ve = new Ve
                {
                    VeId = Guid.NewGuid().ToString(),
                    DonHangId = donHang.DonHangId,
                    GheID = gheDb.GheID,
                    Gia = req.GiaVe
                };

                _context.DonHang.Add(donHang);
                _context.Ve.Add(ve);
                await _context.SaveChangesAsync();

                return new KetQuaBanVe { Success = true, Message = "Xuất vé thành công!" };
            }
            catch (Exception ex)
            {
                return new KetQuaBanVe { Success = false, Message = "Lỗi: " + ex.Message };
            }
        }

        // --- HÀM HỦY VÉ (ĐÃ SỬA LẠI LOGIC TÌM GHẾ) ---
        public async Task<KetQuaBanVe> HuyVeAsync(string chuyenId, string soGhe, string nhaXeId)
        {
            var ve = await _context.Ve
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.Xe)
                .Include(v => v.Ghe) // Include bảng Ghe để so sánh số ghế
                .FirstOrDefaultAsync(v => v.DonHang.ChuyenId == chuyenId
                                      && v.Ghe.SoGhe == soGhe // Sửa: So sánh chuỗi "A01" với cột SoGhe
                                      && v.DonHang.TrangThaiThanhToan != "Da huy");

            if (ve == null) return new KetQuaBanVe { Success = false, Message = "Không tìm thấy vé này hoặc vé đã hủy." };
            if (ve.DonHang.ChuyenXe.Xe.NhaXeId != nhaXeId) return new KetQuaBanVe { Success = false, Message = "Bạn không có quyền hủy vé của nhà xe khác." };

            ve.DonHang.TrangThaiThanhToan = "Da huy";
            await _context.SaveChangesAsync();

            return new KetQuaBanVe { Success = true, Message = "Đã hủy vé thành công." };
        }
        // --- TÌM KIẾM VÉ ---
        public async Task<VeBanInfo> TraCuuVeAsync(string keyword, string nhaXeId)
        {
            // Tìm trong DB (Join bảng Ve, DonHang, Ghe, ChuyenXe)
            var ve = await _context.Ve
                .Include(v => v.Ghe)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.LoTrinh.TramDiNavigation)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.LoTrinh.TramToiNavigation)
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe.Xe)
                .Where(v => v.DonHang.ChuyenXe.Xe.NhaXeId == nhaXeId) // Chỉ tìm vé của nhà xe mình
                .FirstOrDefaultAsync(v => v.VeId == keyword
                                       || v.DonHangId == keyword
                                       || v.DonHang.SdtNguoiDat == keyword); // Tìm theo 3 tiêu chí

            if (ve == null) return null;

            return new VeBanInfo
            {
                GheID = ve.VeId, // Tạm dùng field này để trả về VeId cho tiện
                SoGhe = ve.Ghe.SoGhe,
                TenKhach = ve.DonHang.HoTenNguoiDat,
                Sdt = ve.DonHang.SdtNguoiDat,
                TrangThai = ve.DonHang.TrangThaiThanhToan,
                ChuyenId = ve.DonHang.ChuyenId,
                // Lợi dụng các field string thừa để trả về thông tin lộ trình (DTO hơi "lười" tí nhưng tiện)
                // Trong dự án thật nên tạo DTO riêng cho chi tiết
            };
        }

        // --- ĐỔI GHẾ ---
        public async Task<KetQuaBanVe> DoiGheAsync(string veId, string soGheMoi, string nhaXeId)
        {
            // 1. Lấy thông tin vé cũ
            var veCu = await _context.Ve
                .Include(v => v.DonHang).ThenInclude(dh => dh.ChuyenXe)
                .FirstOrDefaultAsync(v => v.VeId == veId);

            if (veCu == null) return new KetQuaBanVe { Success = false, Message = "Không tìm thấy vé." };

            // 2. Kiểm tra ghế mới (Số ghế mới phải tồn tại và TRỐNG ở chuyến đó)
            // Lưu ý: Dùng logic tạo ghế tự động hoặc tìm ghế có sẵn
            // Ở đây ta tìm ghế có sẵn (vì chuyến này chắc chắn đã bán vé rồi nên đã có danh sách ghế)
            var gheMoi = await _context.Ghe
                .FirstOrDefaultAsync(g => g.XeId == veCu.DonHang.ChuyenXe.XeId && g.SoGhe == soGheMoi);

            if (gheMoi == null) return new KetQuaBanVe { Success = false, Message = $"Ghế {soGheMoi} không tồn tại trên xe này." };

            // 3. Check xem ghế mới có ai ngồi chưa
            bool daCoNguoiDat = await _context.Ve.AnyAsync(v =>
                v.DonHang.ChuyenId == veCu.DonHang.ChuyenId &&
                v.GheID == gheMoi.GheID &&
                v.DonHang.TrangThaiThanhToan != "Da huy");

            if (daCoNguoiDat) return new KetQuaBanVe { Success = false, Message = $"Ghế {soGheMoi} đã có người khác đặt rồi." };

            // 4. Cập nhật vé sang ghế mới
            veCu.GheID = gheMoi.GheID;

            await _context.SaveChangesAsync();
            return new KetQuaBanVe { Success = true, Message = $"Đổi thành công sang ghế {soGheMoi}." };
        }
    }
}