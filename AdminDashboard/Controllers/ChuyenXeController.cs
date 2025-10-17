using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class ChuyenXeController : Controller
    {
        private readonly Db27524Context _context; // Thay Db27524Context bằng tên DbContext của bạn nếu khác

        public ChuyenXeController(Db27524Context context)
        {
            _context = context;
        }

        // GET: ChuyenXe
        public async Task<IActionResult> Index()
        {
            var chuyenXes = await _context.ChuyenXe
         .Include(c => c.LoTrinh)
             .ThenInclude(lt => lt.TramDiNavigation)
         .Include(c => c.LoTrinh)
             .ThenInclude(lt => lt.TramToiNavigation)
         .Include(c => c.Xe)
          .Include(c => c.TaiXe)
         .ToListAsync();

            return View(chuyenXes);
        }

        // GET: ChuyenXe/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);

            if (chuyenXe == null) return NotFound();

            return View(chuyenXe);
        }

        // GET: ChuyenXe/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }


        // POST: ChuyenXe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
        {
            // Loại bỏ validation các navigation property
            ModelState.Remove("ChuyenId");
            ModelState.Remove("LoTrinh");
            ModelState.Remove("Xe");
            ModelState.Remove("TaiXe");


            if (ModelState.IsValid)
            {
                chuyenXe.ChuyenId = Guid.NewGuid().ToString("N").Substring(0, 8);

                if (chuyenXe.TrangThai == 0)
                    chuyenXe.TrangThai = TrangThaiChuyenXe.DaLenLich;

                _context.Add(chuyenXe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Đã tạo chuyến xe thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("❌ ModelState Error: " + err.ErrorMessage);
            }

            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
            return View(chuyenXe);
        }

        // GET: ChuyenXe/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var chuyenXe = await _context.ChuyenXe.FindAsync(id);
            if (chuyenXe == null) return NotFound();

            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
            return View(chuyenXe);
        }


       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ChuyenId,LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe)
        {
            if (id != chuyenXe.ChuyenId) return NotFound();

            ModelState.Remove("LoTrinh");
            ModelState.Remove("Xe");
            ModelState.Remove("TaiXe");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuyenXe);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật chuyến xe thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ChuyenXe.Any(e => e.ChuyenId == chuyenXe.ChuyenId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
            return View(chuyenXe);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);

            if (chuyenXe == null) return NotFound();

            return View(chuyenXe);
        }

        // ✅ POST: ChuyenXe/Delete/5 (ĐÃ SỬA)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string ChuyenId)
        {
            var chuyen = await _context.ChuyenXe.FindAsync(ChuyenId);
            if (chuyen == null)
                return NotFound();

            _context.ChuyenXe.Remove(chuyen);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa chuyến xe thành công!";
            return RedirectToAction(nameof(Index));
        }


        // Hàm hỗ trợ để lấy dữ liệu cho dropdown, tránh lặp code
        private void PopulateDropdownLists(object selectedLoTrinh = null, object selectedXe = null)
        {
            var loTrinhs = _context.LoTrinh
                .Include(lt => lt.TramDiNavigation)
                .Include(lt => lt.TramToiNavigation)
                .ToList();

            var loTrinhDisplay = loTrinhs.Select(lt => new
            {
                LoTrinhId = lt.LoTrinhId,
                Name = lt.TramDiNavigation.TenTram + " - " + lt.TramToiNavigation.TenTram
            }).ToList();

            ViewBag.LoTrinhId = new SelectList(loTrinhDisplay, "LoTrinhId", "Name", selectedLoTrinh);

            ViewBag.XeId = new SelectList(_context.Xe.ToList(), "XeId", "BienSoXe", selectedXe);
        }


        public async Task<IActionResult> AssignDriver(string id)
        {
            if (id == null) return NotFound();

            var chuyenXeCanPhanCong = await _context.ChuyenXe
                .Include(c => c.Xe)
        .Include(c => c.LoTrinh) // Tải Lộ Trình
            .ThenInclude(lt => lt.TramDiNavigation) // Tải Trạm Đi BÊN TRONG Lộ Trình
        .Include(c => c.LoTrinh) // Phải Include lại để ThenInclude tiếp cho thuộc tính khác
            .ThenInclude(lt => lt.TramToiNavigation) // Tải Trạm Tới BÊN TRONG Lộ Trình
        .FirstOrDefaultAsync(c => c.ChuyenId == id);
            if (chuyenXeCanPhanCong == null) return NotFound();

            // --- LOGIC TÌM TÀI XẾ RẢNH ---
            // 1. Lấy danh sách ID của TẤT CẢ các tài xế (logic này vẫn đúng)
            var taiXeRoleId = await _context.VaiTro.Where(r => r.TenVaiTro == "TaiXe").Select(r => r.RoleId).FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(taiXeRoleId))
            {
                // Xử lý trường hợp không có vai trò tài xế
                ViewBag.ErrorMessage = "Không tìm thấy vai trò 'TaiXe' trong hệ thống.";
                ViewBag.AvailableDrivers = new SelectList(new List<NguoiDung>());
                return View(chuyenXeCanPhanCong);
            }
            var allDriverIds = await _context.UserRole.Where(ur => ur.RoleId == taiXeRoleId).Select(ur => ur.UserId).ToListAsync();

            // 2. Tính toán trước thời gian bắt đầu và kết thúc của chuyến xe cần phân công
            var thoiGianBatDau = chuyenXeCanPhanCong.NgayDi.Add(chuyenXeCanPhanCong.GioDi);
            var thoiGianKetThuc = chuyenXeCanPhanCong.NgayDi.Add(chuyenXeCanPhanCong.GioDenDuKien);

            // 3. Lọc trước các chuyến xe có khả năng bị trùng lặp trên DATABASE
            // Chỉ lấy các chuyến xe có tài xế và diễn ra trong cùng một ngày
            var potentialConflicts = await _context.ChuyenXe
                .Where(cx => cx.ChuyenId != id &&
                             cx.TaiXeId != null &&
                             cx.NgayDi.Date == chuyenXeCanPhanCong.NgayDi.Date)
                .ToListAsync(); // Tải danh sách nhỏ này về client

            // 4. Lọc chi tiết các tài xế bị trùng lịch bằng C# (CLIENT-SIDE)
            // Bây giờ, phép toán .Add() sẽ chạy trên C# và không còn lỗi
            var conflictingDriverIds = potentialConflicts
                .Where(cx => cx.NgayDi.Add(cx.GioDi) < thoiGianKetThuc &&
                             cx.NgayDi.Add(cx.GioDenDuKien) > thoiGianBatDau)
                .Select(cx => cx.TaiXeId)
                .Distinct()
                .ToList();

            // 5. Lấy ra danh sách các tài xế KHÔNG BỊ TRÙNG LỊCH 
            var availableDriverIds = allDriverIds.Except(conflictingDriverIds).ToList();

            var availableDrivers = await _context.NguoiDung
                .Where(u => availableDriverIds.Contains(u.UserId))
                .ToListAsync();


            ViewBag.AvailableDrivers = new SelectList(availableDrivers, "UserId", "HoTen");
            return View(chuyenXeCanPhanCong);
        }

        // POST: ChuyenXe/AssignDriver/chuyen-xe-id-123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(string ChuyenId, string TaiXeId)
        {
            if (ChuyenId == null || TaiXeId == null) return BadRequest("Thông tin không hợp lệ.");

            var chuyenXe = await _context.ChuyenXe.FindAsync(ChuyenId);
            if (chuyenXe == null) return NotFound();

            // Gán tài xế vào chuyến
            chuyenXe.TaiXeId = TaiXeId;

            // Cập nhật trạng thái chuyến xe (nếu cần)
            // Ví dụ: Sau khi có tài xế, chuyển sang "Chờ Khởi Hành"
            if (chuyenXe.TrangThai == TrangThaiChuyenXe.DaLenLich)
            {
                chuyenXe.TrangThai = TrangThaiChuyenXe.ChoKhoiHanh;
            }

            _context.Update(chuyenXe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã phân công tài xế thành công!";
            return RedirectToAction(nameof(Index));
        }






        public IActionResult TimKiem(string diemDiId, string diemDenId, DateTime ngayDi)
        {// Truy vấn CSDL để lấy các chuyến xe phù hợp
            var ketQua = _context.ChuyenXe
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation) // Lấy thông tin trạm đi
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation) // Lấy thông tin trạm tới
                .Include(c => c.Xe)
                    .ThenInclude(x => x.LoaiXe) // Lấy thông tin loại xe
                .Where(c => c.LoTrinh.TramDi == diemDiId &&
                            c.LoTrinh.TramToi == diemDenId &&
                            c.NgayDi.Date == ngayDi.Date)
                .OrderBy(c => c.GioDi) // Sắp xếp theo giờ đi sớm nhất
                .ToList();

            // Lấy tên trạm đi, trạm đến và ngày đi để hiển thị lại cho người dùng trên trang kết quả
            var tramDi = _context.Tram.Find(diemDiId);
            var tramDen = _context.Tram.Find(diemDenId);

            if (tramDi != null) ViewBag.DiemDi = tramDi.TenTram;
            if (tramDen != null) ViewBag.DiemDen = tramDen.TenTram;
            ViewBag.NgayDi = ngayDi.ToString("dd/MM/yyyy");

            // Trả về View "TimKiem" và truyền danh sách kết quả tìm được
            return View(ketQua);
        }

    }


}
