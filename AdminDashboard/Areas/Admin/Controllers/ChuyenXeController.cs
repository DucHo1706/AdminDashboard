﻿using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Cần có thư viện này để dùng DbUpdateException

namespace AdminDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChuyenXeController : Controller
    {
        private readonly Db27524Context _context; // Thay Db27524Context bằng tên DbContext của bạn nếu khác
        private readonly IImageService _imageService;
        public ChuyenXeController(Db27524Context context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }
        public async Task<IActionResult> Index()
        {
            var chuyenXes = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Include(c => c.TaiXe)
                .Include(c => c.Images) 
                .OrderByDescending(c => c.NgayDi)
                .ToListAsync();

            await PopulateTramDropdowns();

           
            return View(chuyenXes);
        }

        // Hàm hỗ trợ nạp Dropdown Trạm
        private async Task PopulateTramDropdowns()
        {
            var tramList = await _context.Tram.OrderBy(t => t.TenTram).ToListAsync();

            // DÙNG IdTram LÀM VALUE, TenTram LÀM TEXT
            ViewBag.TramDiList = new SelectList(tramList, "IdTram", "TenTram");
            ViewBag.TramDenList = new SelectList(tramList, "IdTram", "TenTram");
        }


        //  ACTION TÌM KIẾM AJAX BẰNG ID
        [HttpGet]
        public async Task<IActionResult> TimKiemAjax(string diemDi, string diemDen)
        {
            try
            {
                Console.WriteLine($"🔹 TimKiemAjax => ID Trạm Đi={diemDi}, ID Trạm Đến={diemDen}");

                var query = _context.ChuyenXe
                    .Include(c => c.LoTrinh)
                        .ThenInclude(lt => lt.TramDiNavigation)
                    .Include(c => c.LoTrinh)
                        .ThenInclude(lt => lt.TramToiNavigation)
                    .Include(c => c.Xe)
                    .Include(c => c.Images) // <-- THÊM DÒNG NÀY ĐỂ TẢI ẢNH
                    .AsQueryable();

                if (!string.IsNullOrEmpty(diemDi))
                {
                    query = query.Where(c => c.LoTrinh.TramDi == diemDi);
                }

                if (!string.IsNullOrEmpty(diemDen))
                {
                    query = query.Where(c => c.LoTrinh.TramToi == diemDen);
                }

                if (string.IsNullOrEmpty(diemDi) && string.IsNullOrEmpty(diemDen))
                {
                    query = query.Where(c => 1 == 0);
                }

                var ketQua = await query.OrderBy(c => c.NgayDi).ThenBy(c => c.GioDi).ToListAsync();

                return PartialView("_BangChuyenXe", ketQua);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Lỗi server: {ex.Message}");
                return Content($"<div class='alert alert-danger mt-3'>Đã xảy ra lỗi: {ex.Message}</div>");
            }
        }
        //// GET: ChuyenXe/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null) return NotFound();

        //var chuyenXe = await _context.ChuyenXe
        //    .Include(c => c.LoTrinh)
        //        .ThenInclude(lt => lt.TramDiNavigation) // Tải Trạm Đi
        //    .Include(c => c.LoTrinh)
        //        .ThenInclude(lt => lt.TramToiNavigation) // Tải Trạm Đến
        //    .Include(c => c.Xe)
        //    .Include(c => c.TaiXe)
        //    .FirstOrDefaultAsync(m => m.ChuyenId == id);

        //    if (chuyenXe == null) return NotFound();

        //    return View(chuyenXe);
        //}

        // GET: ChuyenXe/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // ✅ POST: ChuyenXe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoTrinhId,XeId,NgayDi,GioDi,GioDenDuKien,TrangThai")] ChuyenXe chuyenXe, IFormFileCollection images)
        {
            ModelState.Remove("ChuyenId");
            ModelState.Remove("LoTrinh");
            ModelState.Remove("Xe");
            ModelState.Remove("TaiXe");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                chuyenXe.ChuyenId = Guid.NewGuid().ToString("N")[..8];
                chuyenXe.TrangThai = chuyenXe.TrangThai == 0 ? TrangThaiChuyenXe.DaLenLich : chuyenXe.TrangThai;

                _context.Add(chuyenXe);
                await _context.SaveChangesAsync();

                // Upload ảnh (nếu có)
                if (images?.Count > 0)
                {
                    var imageUrls = await _imageService.UploadImagesAsync(images);
                    foreach (var url in imageUrls)
                    {
                        _context.ChuyenXeImage.Add(new ChuyenXeImage
                        {
                            ChuyenId = chuyenXe.ChuyenId,
                            ImageUrl = url
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "✅ Đã tạo chuyến xe thành công!";
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
            return View(chuyenXe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            // BƯỚC 1: Tìm ảnh trong CSDL
            var image = await _context.ChuyenXeImage.FindAsync(imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ảnh." });
            }

            // Lưu lại URL để dùng sau khi CSDL đã xóa
            string imageUrlToDelete = image.ImageUrl;

            try
            {
                // BƯỚC 2: Đánh dấu bản ghi ảnh để xóa
                _context.ChuyenXeImage.Remove(image);

                // BƯỚC 3: Lưu thay đổi vào CSDL
                // ⭐️ NẾU CÓ LỖI, NÓ SẼ BỊ VĂNG RA Ở DÒNG NÀY ⭐️
                await _context.SaveChangesAsync();

                // BƯỚC 4: Xóa file vật lý (Cloudinary/Local)
                // Chỉ xóa file vật lý SAU KHI đã xóa CSDL thành công
                await _imageService.DeleteImageAsync(imageUrlToDelete);

                return Json(new { success = true, message = "Xóa ảnh thành công!" });
            }
            catch (DbUpdateException dbEx) // BƯỚC 5: BẮT LỖI RÀNG BUỘC CSDL
            {
                // Lấy thông báo lỗi chi tiết
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                // Ghi log lỗi ra Console Server (Đây là thứ chúng ta cần)
                Console.WriteLine($"❌ LỖI RÀNG BUỘC DB KHI XÓA ẢNH: {errorMessage}");

                // Trả lỗi về cho client
                return Json(new
                {
                    success = false,
                    message = "Lỗi CSDL: Không thể xóa do có ràng buộc khóa ngoại.",
                    errorDetail = errorMessage // Gửi kèm chi tiết lỗi
                });
            }
            catch (Exception ex) // Bắt các lỗi chung khác
            {
                Console.WriteLine($"❌ LỖI KHÁC KHI XÓA ẢNH: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi không xác định xảy ra." });
            }
        }
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh)
                    .ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe)
                .Include(c => c.TaiXe)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);

            if (chuyenXe == null) return NotFound();

            return View(chuyenXe);
        }



        // GET: ChuyenXe/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenXe = _context.ChuyenXe
                .Include(c => c.Images)
                .FirstOrDefault(c => c.ChuyenId == id);

            if (chuyenXe == null)
            {
                return NotFound();
            }

            PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
            return View(chuyenXe);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChuyenXe chuyenXe, string deletedImages, IFormFileCollection newImages)
        {
            var existing = await _context.ChuyenXe
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenXe.ChuyenId);

            if (existing == null) return NotFound();

            var strategy = _context.Database.CreateExecutionStrategy();

            List<string> urlsToDelete = new List<string>();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    existing.LoTrinhId = chuyenXe.LoTrinhId;
                    existing.XeId = chuyenXe.XeId;
                    existing.NgayDi = chuyenXe.NgayDi;
                    existing.GioDi = chuyenXe.GioDi;
                    existing.GioDenDuKien = chuyenXe.GioDenDuKien;
                    existing.TrangThai = chuyenXe.TrangThai;

                    if (newImages != null && newImages.Any())
                    {
                        var imageUrls = await _imageService.UploadImagesAsync(newImages);
                        foreach (var url in imageUrls)
                        {
                            existing.Images.Add(new ChuyenXeImage { ChuyenId = existing.ChuyenId, ImageUrl = url });
                        }
                    }

                    if (!string.IsNullOrEmpty(deletedImages))
                    {
                        var ids = deletedImages.Split(',').Select(int.Parse).ToList();
                        var toRemove = existing.Images.Where(i => ids.Contains(i.ImageId)).ToList();

                        foreach (var image in toRemove)
                        {
                            urlsToDelete.Add(image.ImageUrl);
                            existing.Images.Remove(image);
                        }
                    }

                    await _context.SaveChangesAsync();
                });

                foreach (var url in urlsToDelete)
                {
                    await _imageService.DeleteImageAsync(url);
                }

                TempData["SuccessMessage"] = "Cập nhật chuyến xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"LỖI CSDL KHI EDIT: {errorMessage}");
                TempData["ErrorMessage"] = $"Lỗi CSDL khi xóa ảnh: {errorMessage}";

                PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
                return View(chuyenXe);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI KHÁC KHI EDIT: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi không xác định: {ex.Message}";

                PopulateDropdownLists(chuyenXe.LoTrinhId, chuyenXe.XeId);
                return View(chuyenXe);
            }
        }
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.LoTrinh)
                .Include(c => c.Xe)
                .FirstOrDefaultAsync(m => m.ChuyenId == id);

            if (chuyenXe == null) return NotFound();

            return View(chuyenXe);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var chuyenXe = await _context.ChuyenXe
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == id);

            if (chuyenXe == null) return NotFound();

            if (chuyenXe.TrangThai != TrangThaiChuyenXe.DaLenLich)
            {
                TempData["ErrorMessage"] = $"Lỗi: Chỉ có thể xóa chuyến xe 'Đã Lên Lịch'.";
                return RedirectToAction(nameof(Index));
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            List<string> urlsToDelete = chuyenXe.Images.Select(i => i.ImageUrl).ToList();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    chuyenXe.Images.Clear();
                    _context.ChuyenXe.Remove(chuyenXe);
                    await _context.SaveChangesAsync();
                });

                foreach (var url in urlsToDelete)
                {
                    await _imageService.DeleteImageAsync(url);
                }

                TempData["SuccessMessage"] = "Đã xóa chuyến xe và các ảnh liên quan thành công!";
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"LỖI KHÔNG THỂ XÓA (DB): {errorMessage}");
                TempData["ErrorMessage"] = $"Lỗi: Không thể xóa, có ràng buộc dữ liệu. ({errorMessage})";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi không xác định: {ex.Message}";
            }

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
                lt.LoTrinhId,
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
                            c.NgayDi.Date == ngayDi.Date &&
                            c.TrangThai == TrangThaiChuyenXe.DangMoBanVe)

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



        // POST: ChuyenXe/MoBanVe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoBanVe(string id)
        {
            var chuyenXe = await _context.ChuyenXe.FindAsync(id);
            if (chuyenXe == null)
            {
                return NotFound();
            }

            if (chuyenXe.TrangThai == TrangThaiChuyenXe.DaLenLich)
            {
                chuyenXe.TrangThai = TrangThaiChuyenXe.DangMoBanVe;
                _context.Update(chuyenXe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = " Đã mở bán vé cho chuyến xe thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = " Chỉ có thể mở bán vé cho chuyến xe 'Đã Lên Lịch'.";
            }

            return RedirectToAction(nameof(Index));
        }

    }


}