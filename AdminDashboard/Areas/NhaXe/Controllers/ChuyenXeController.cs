using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Areas.NhaXe.Controllers
{
    [Area("NhaXe")]
     [Authorize(Roles = "ChuNhaXe")]
    public class ChuyenXeController : Controller
    {
        private readonly Db27524Context _context;
        private readonly IChuyenXeService _chuyenXeService;
        private readonly IChuyenXePrototypeService _chuyenXePrototypeService;

        public ChuyenXeController(Db27524Context context, IChuyenXeService chuyenXeService, IChuyenXePrototypeService chuyenXePrototypeService)
        {
            _context = context;
            _chuyenXeService = chuyenXeService;
            _chuyenXePrototypeService = chuyenXePrototypeService;
        }

        private string NhaXeId => User.FindFirst("NhaXeId")?.Value;

        // 1. DANH SÁCH (Giữ nguyên logic hiển thị vì nó thuộc về View)
        public async Task<IActionResult> Index(int page = 1, int? trangThai = null)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return RedirectToAction("Login", "Auth", new { area = "" });

            const int pageSize = 50;
            var query = _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe).Include(c => c.TaiXe).Include(c => c.Images)
                .Where(c => c.Xe.NhaXeId == NhaXeId);

            if (trangThai.HasValue) query = query.Where(c => (int)c.TrangThai == trangThai.Value);
            query = query.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi);

            var totalRecords = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CurrentTrangThai = trangThai;

            await LoadTramDropdownForSearch();
            return View(data);
        }

        // 2. TẠO MỚI (Đã gọn)
        public IActionResult Create() { LoadDropdowns(); return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoLichChayRequest request)
        {
            if (request.TuNgay > request.DenNgay) ModelState.AddModelError("", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _chuyenXeService.TaoLichTuDongAsync(request, NhaXeId);
                    if (result.Message != "Success") ModelState.AddModelError("", result.Message);
                    else
                    {
                        TempData["SuccessMessage"] = result.Skipped > 0
                            ? $"Tạo {result.Success} chuyến. Bỏ qua {result.Skipped} chuyến trùng."
                            : $"Thành công {result.Success} chuyến!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message); }
            }
            LoadDropdowns(request.LoTrinhId, request.XeId);
            return View(request);
        }

        // Nhan Ban
        [HttpGet]
        public async Task<IActionResult> NhanBan(string id)
        {
            if (string.IsNullOrEmpty(NhaXeId))
                return RedirectToAction("Login", "Auth", new { area = "" });

            var sourceTrip = await _chuyenXePrototypeService.GetSourceTripAsync(id, NhaXeId);
            if (sourceTrip == null) return NotFound();

            var model = new NhanBanChuyenXeRequest
            {
                SourceChuyenId = sourceTrip.ChuyenId,
                NgayDi = sourceTrip.NgayDi.AddDays(1),
                GioDi = sourceTrip.GioDi,
                GioDenDuKien = sourceTrip.GioDenDuKien,
                XeId = sourceTrip.XeId,

                // Để trống dropdown tài xế; checkbox bên dưới quyết định có sao chép tài xế cũ hay không
                TaiXeId = null,

                SaoChepHinhAnh = sourceTrip.Images.Any(),
                SaoChepTaiXe = !string.IsNullOrWhiteSpace(sourceTrip.TaiXeId),
                ResetTrangThaiChoDuyet = true
            };

            await LoadPrototypeDropdownsAsync(model.XeId, model.TaiXeId);
            ViewBag.SourceTrip = sourceTrip;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhanBan(NhanBanChuyenXeRequest request)
        {
            if (string.IsNullOrEmpty(NhaXeId))
                return RedirectToAction("Login", "Auth", new { area = "" });

            var sourceTrip = await _chuyenXePrototypeService.GetSourceTripAsync(request.SourceChuyenId, NhaXeId);
            if (sourceTrip == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadPrototypeDropdownsAsync(request.XeId, request.TaiXeId);
                ViewBag.SourceTrip = sourceTrip;
                return View(request);
            }

            var result = await _chuyenXePrototypeService.NhanBanTuMauAsync(request, NhaXeId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await LoadPrototypeDropdownsAsync(request.XeId, request.TaiXeId);
            ViewBag.SourceTrip = sourceTrip;
            return View(request);
        }
        // 3. EDIT (GET) - Giữ nguyên logic lấy dữ liệu để hiển thị
        public async Task<IActionResult> Edit(string id)
        {
            var cx = await GetSecureChuyenXeForView(id);
            if (cx == null) return NotFound();
            LoadDropdowns(cx.LoTrinhId, cx.XeId);
            return View(cx);
        }

        // 3. EDIT (POST) - ĐÃ CHUYỂN LOGIC SANG SERVICE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChuyenXe model, string deletedImages, IFormFileCollection newImages)
        {
            try
            {
                // Gọi Service xử lý tất cả (Update info + Ảnh + Check quyền)
                string result = await _chuyenXeService.UpdateChuyenXeAsync(model, deletedImages, newImages, NhaXeId);

                if (result == "Success")
                {
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else if (result == "Warning:Reapproval")
                {
                    TempData["WarningMessage"] = "Cập nhật thành công, nhưng chuyến xe cần Admin duyệt lại do thay đổi lịch trình.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = result; // Lỗi từ service trả về
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // 4. DELETE (GET)
        public async Task<IActionResult> Delete(string id)
        {
            var cx = await GetSecureChuyenXeForView(id);
            return cx == null ? NotFound() : View(cx);
        }

        // 4. DELETE (POST) - ĐÃ CHUYỂN LOGIC SANG SERVICE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            string result = await _chuyenXeService.DeleteChuyenXeAsync(id, NhaXeId);

            if (result == "Success")
                TempData["SuccessMessage"] = "Đã xóa chuyến xe.";
            else
                TempData["ErrorMessage"] = result;

            return RedirectToAction(nameof(Index));
        }

        // Helper riêng cho Controller để load dữ liệu hiển thị (GET)
        private async Task<ChuyenXe?> GetSecureChuyenXeForView(string id)
        {
            var cx = await _context.ChuyenXe
                .Include(c => c.Xe).Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == id);
            if (cx == null || cx.Xe.NhaXeId != NhaXeId) return null;
            return cx;
        }

        private void LoadDropdowns(object selLoTrinh = null, object selXe = null)
        {
            var loTrinhs = _context.LoTrinh.Select(lt => new { lt.LoTrinhId, Name = $"{lt.TramDiNavigation.TenTram} -> {lt.TramToiNavigation.TenTram}" }).ToList();
            var xes = _context.Xe.Where(x => x.NhaXeId == NhaXeId).Select(x => new { x.XeId, x.BienSoXe }).ToList();
            ViewBag.LoTrinhId = new SelectList(loTrinhs, "LoTrinhId", "Name", selLoTrinh);
            ViewBag.XeId = new SelectList(xes, "XeId", "BienSoXe", selXe);
        }

        private async Task LoadPrototypeDropdownsAsync(object? selXe = null, object? selTaiXe = null)
        {
            var xes = await _context.Xe
                .Include(x => x.LoaiXe)
                .Where(x => x.NhaXeId == NhaXeId)
                .OrderBy(x => x.BienSoXe)
                .Select(x => new
                {
                    x.XeId,
                    Name = x.BienSoXe + " - " + (x.LoaiXe != null ? x.LoaiXe.TenLoaiXe : "Chưa rõ loại")
                })
                .ToListAsync();

            var taiXes = await _context.NhanVien
                .Where(nv => nv.NhaXeId == NhaXeId
                             && nv.VaiTro == VaiTroNhanVien.TaiXe
                             && nv.DangLamViec)
                .OrderBy(nv => nv.HoTen)
                .Select(nv => new
                {
                    nv.NhanVienId,
                    Name = nv.HoTen + " - " + nv.SoDienThoai
                })
                .ToListAsync();

            ViewBag.PrototypeXeId = new SelectList(xes, "XeId", "Name", selXe);
            ViewBag.PrototypeTaiXeId = new SelectList(taiXes, "NhanVienId", "Name", selTaiXe);
        }

        private async Task LoadTramDropdownForSearch()
        {
            var tramList = await _context.Tram.OrderBy(t => t.TenTram).Select(t => new { t.IdTram, t.TenTram }).ToListAsync();
            ViewBag.TramDiList = new SelectList(tramList, "IdTram", "TenTram");
            ViewBag.TramDenList = new SelectList(tramList, "IdTram", "TenTram");
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemAjax(string diemDi, string diemDen, int? trangThai, int page = 1)
        {
            if (string.IsNullOrEmpty(NhaXeId)) return Unauthorized();
            const int pageSize = 50;
            var query = _context.ChuyenXe.Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation).Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation).Include(c => c.Xe).Include(c => c.TaiXe).Include(c => c.Images).Where(c => c.Xe.NhaXeId == NhaXeId).AsQueryable();
            if (!string.IsNullOrEmpty(diemDi)) query = query.Where(c => c.LoTrinh.TramDi == diemDi);
            if (!string.IsNullOrEmpty(diemDen)) query = query.Where(c => c.LoTrinh.TramToi == diemDen);
            if (trangThai.HasValue) query = query.Where(c => (int)c.TrangThai == trangThai.Value);
            var totalRecords = await query.CountAsync();
            var data = await query.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CurrentTrangThai = trangThai;
            return PartialView("_BangChuyenXe", data);
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailsPartial(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var cx = await _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe).Include(c => c.Xe) // Include thêm loại xe nếu cần
                .Include(c => c.TaiXe)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == id);

            if (cx == null) return NotFound();

            return PartialView("_ChuyenXeDetailsModal", cx);
        }

    }
}