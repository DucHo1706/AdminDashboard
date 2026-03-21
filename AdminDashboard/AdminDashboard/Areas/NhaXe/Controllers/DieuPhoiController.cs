using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Services; // Thêm namespace này
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
    [Authorize(Roles = "ChuNhaXe")]
    public class DieuPhoiController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IChuyenXeService _chuyenXeService; 

        // Cập nhật Constructor
        public DieuPhoiController(Db27524Context context, IChuyenXeService chuyenXeService)
        {
            _context = context;
            _chuyenXeService = chuyenXeService;
        }

        private string NhaXeId => User.FindFirst("NhaXeId")?.Value;

        public async Task<IActionResult> Index(DateTime? date)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return RedirectToAction("Login", "Auth", new { area = "" });

            var startDate = date ?? DateTime.Today;
            var endDate = startDate.AddDays(6);

            // 1. Lấy Tài xế (Giữ nguyên)
            var taiXes = await _context.NhanVien
                .Where(nv => nv.NhaXeId == NhaXeId && nv.VaiTro == VaiTroNhanVien.TaiXe && nv.DangLamViec)
                .ToListAsync();

            var assignedTrips = await _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Where(c => c.Xe.NhaXeId == NhaXeId
                            && c.NgayDi >= startDate && c.NgayDi <= endDate
                            && c.TaiXeId != null) // Đã có tài xế
                .ToListAsync();

            var unassignedTrips = await _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Where(c => c.Xe.NhaXeId == NhaXeId
                            && c.NgayDi >= startDate && c.NgayDi <= endDate
                            && c.TaiXeId == null // <--- Chưa có tài xế
                            && c.TrangThai != TrangThaiChuyenXe.DaHuy)
                .OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi)
                .ToListAsync();

            var model = new LichLamViecViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TaiXes = new List<TaiXeLichRow>(),

                UnassignedTrips = unassignedTrips.Select(t => new ChuyenXeShort
                {
                    ChuyenId = t.ChuyenId,
                    BienSoXe = t.Xe.BienSoXe,
                    Tuyen = $"{t.LoTrinh.TramDiNavigation.TenTram} - {t.LoTrinh.TramToiNavigation.TenTram}",
                    GioDi = $"{t.GioDi:hh\\:mm} ({t.NgayDi:dd/MM})", // Hiện cả giờ và ngày
                    TrangThai = (int)t.TrangThai
                }).ToList()
            };

            foreach (var tx in taiXes)
            {
                var row = new TaiXeLichRow
                {
                    TaiXeId = tx.NhanVienId,
                    HoTen = tx.HoTen,
                    SoDienThoai = tx.SoDienThoai,
                    LichTrinh = new Dictionary<string, List<ChuyenXeShort>>()
                };

                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    var dateKey = d.ToString("yyyy-MM-dd");
                    var tripsInDay = assignedTrips
                        .Where(t => t.TaiXeId == tx.NhanVienId && t.NgayDi.Date == d.Date)
                        .OrderBy(t => t.GioDi)
                        .Select(t => new ChuyenXeShort
                        {
                            ChuyenId = t.ChuyenId,
                            BienSoXe = t.Xe.BienSoXe,
                            Tuyen = $"{t.LoTrinh.TramDiNavigation.TenTram}-{t.LoTrinh.TramToiNavigation.TenTram}",
                            GioDi = t.GioDi.ToString(@"hh\:mm"),
                            TrangThai = (int)t.TrangThai
                        }).ToList();
                    row.LichTrinh.Add(dateKey, tripsInDay);
                }
                model.TaiXes.Add(row);
            }
            return View(model);
        }

        // --- 2. CHỨC NĂNG PHÂN CÔNG (CHUYỂN TỪ CHUYENXECONTROLLER SANG) ---

        [HttpGet]
        public async Task<IActionResult> PhanCong(string id)
        {
            // Lấy thông tin chuyến xe
            var cx = await _context.ChuyenXe
                .Include(c => c.Xe)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .FirstOrDefaultAsync(c => c.ChuyenId == id);

            if (cx == null || cx.Xe.NhaXeId != NhaXeId) return NotFound();

            // Lấy danh sách Tài xế
            var taiXes = _context.NhanVien
                .Where(nv => nv.NhaXeId == NhaXeId
                          && nv.VaiTro == AdminDashboard.Models.VaiTroNhanVien.TaiXe
                          && nv.DangLamViec)
                .Select(nv => new { nv.NhanVienId, Info = $"{nv.HoTen} - {nv.SoDienThoai}" })
                .ToList();

            ViewBag.ListTaiXe = new SelectList(taiXes, "NhanVienId", "Info", cx.TaiXeId);

            return View(cx); // Bạn cần di chuyển file View PhanCong.cshtml sang thư mục DieuPhoi
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanCong(string id, string taiXeId)
        {
            string result = await _chuyenXeService.PhanCongTaiXeAsync(id, taiXeId, NhaXeId);

            if (result == "Success")
            {
                TempData["SuccessMessage"] = "Đã cập nhật điều phối thành công!";
                // Quay lại trang bảng lịch để xem kết quả ngay
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = result;
                // Nếu lỗi thì ở lại trang phân công để sửa
                return RedirectToAction(nameof(PhanCong), new { id = id });
            }
        }
    }
}