using AdminDashboard.Models;
using AdminDashboard.Models.TrangThai;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.Patterns.Prototype;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Services
{
    public class ChuyenXePrototypeService : IChuyenXePrototypeService
    {
        private readonly Db27524Context _context;

        public ChuyenXePrototypeService(Db27524Context context)
        {
            _context = context;
        }

        public async Task<ChuyenXe?> GetSourceTripAsync(string chuyenId, string nhaXeId)
        {
            return await _context.ChuyenXe
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramDiNavigation)
                .Include(c => c.LoTrinh).ThenInclude(lt => lt.TramToiNavigation)
                .Include(c => c.Xe).ThenInclude(x => x.LoaiXe)
                .Include(c => c.TaiXe)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == chuyenId && c.Xe.NhaXeId == nhaXeId);
        }

        public async Task<NhanBanChuyenXeResult> NhanBanTuMauAsync(NhanBanChuyenXeRequest request, string nhaXeId)
        {
            var sourceTrip = await _context.ChuyenXe
                .AsNoTracking()
                .Include(c => c.Xe)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.ChuyenId == request.SourceChuyenId && c.Xe.NhaXeId == nhaXeId);

            if (sourceTrip == null)
            {
                return new NhanBanChuyenXeResult
                {
                    Success = false,
                    Message = "Không tìm thấy chuyến gốc hoặc bạn không có quyền sử dụng chuyến này làm mẫu."
                };
            }

            if (request.GioDi == request.GioDenDuKien)
            {
                return new NhanBanChuyenXeResult
                {
                    Success = false,
                    Message = "Giờ đi và giờ đến dự kiến không được trùng nhau."
                };
            }

            var xeHopLe = await _context.Xe.AnyAsync(x => x.XeId == request.XeId && x.NhaXeId == nhaXeId);
            if (!xeHopLe)
            {
                return new NhanBanChuyenXeResult
                {
                    Success = false,
                    Message = "Xe được chọn không thuộc nhà xe của bạn."
                };
            }

            string? taiXeId = ResolveTaiXeId(sourceTrip, request);

            if (!string.IsNullOrWhiteSpace(taiXeId))
            {
                var taiXeHopLe = await _context.NhanVien.AnyAsync(nv =>
                    nv.NhanVienId == taiXeId &&
                    nv.NhaXeId == nhaXeId &&
                    nv.VaiTro == VaiTroNhanVien.TaiXe &&
                    nv.DangLamViec);

                if (!taiXeHopLe)
                {
                    return new NhanBanChuyenXeResult
                    {
                        Success = false,
                        Message = "Tài xế được chọn không hợp lệ hoặc không còn làm việc."
                    };
                }
            }

            var newStart = request.NgayDi.Date.Add(request.GioDi);
            var newEnd = request.NgayDi.Date.Add(request.GioDenDuKien);

            if (request.GioDenDuKien < request.GioDi)
            {
                newEnd = newEnd.AddDays(1);
            }

            var xesGanNgay = await _context.ChuyenXe
                .AsNoTracking()
                .Where(c => c.XeId == request.XeId
                            && c.TrangThai != TrangThaiChuyenXe.DaHuy
                            && c.NgayDi >= request.NgayDi.Date.AddDays(-1)
                            && c.NgayDi <= request.NgayDi.Date.AddDays(1))
                .ToListAsync();

            var xeBiTrungLich = xesGanNgay.Any(c =>
                KhoangThoiGianGiaoNhau(newStart, newEnd, c.NgayDi.Date.Add(c.GioDi), GetTripEnd(c)));

            if (xeBiTrungLich)
            {
                return new NhanBanChuyenXeResult
                {
                    Success = false,
                    Message = "Xe đang bị trùng lịch với chuyến khác trong khung thời gian bạn chọn."
                };
            }

            if (!string.IsNullOrWhiteSpace(taiXeId))
            {
                var chuyenTaiXe = await _context.ChuyenXe
                    .AsNoTracking()
                    .Where(c => c.TaiXeId == taiXeId
                                && c.TrangThai != TrangThaiChuyenXe.DaHuy
                                && c.NgayDi >= request.NgayDi.Date.AddDays(-1)
                                && c.NgayDi <= request.NgayDi.Date.AddDays(1))
                    .ToListAsync();

                var taiXeBiTrung = chuyenTaiXe.Any(c =>
                    KhoangThoiGianGiaoNhau(newStart, newEnd, c.NgayDi.Date.Add(c.GioDi), GetTripEnd(c)));

                if (taiXeBiTrung)
                {
                    return new NhanBanChuyenXeResult
                    {
                        Success = false,
                        Message = "Tài xế đang bị trùng lịch với chuyến khác trong khung thời gian bạn chọn."
                    };
                }
            }

            var cloneOptions = new ChuyenXeCloneOptions
            {
                NgayDi = request.NgayDi,
                GioDi = request.GioDi,
                GioDenDuKien = request.GioDenDuKien,
                XeId = request.XeId,
                TaiXeId = taiXeId,
                SaoChepHinhAnh = request.SaoChepHinhAnh,
                TrangThaiMacDinh = request.ResetTrangThaiChoDuyet
                    ? TrangThaiChuyenXe.ChoDuyet
                    : sourceTrip.TrangThai
            };

            var prototype = new ChuyenXeTemplate(sourceTrip, sourceTrip.Images, cloneOptions);
            var cloned = prototype.Clone();

            _context.ChuyenXe.Add(cloned.ChuyenXe);

            if (cloned.Images.Any())
            {
                _context.ChuyenXeImage.AddRange(cloned.Images);
            }

            await _context.SaveChangesAsync();

            return new NhanBanChuyenXeResult
            {
                Success = true,
                NewChuyenId = cloned.ChuyenXe.ChuyenId,
                Message = $"Đã nhân bản thành công chuyến xe mới: {cloned.ChuyenXe.ChuyenId}"
            };
        }

        private static string? ResolveTaiXeId(ChuyenXe sourceTrip, NhanBanChuyenXeRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.TaiXeId))
            {
                return request.TaiXeId;
            }

            return request.SaoChepTaiXe ? sourceTrip.TaiXeId : null;
        }

        private static DateTime GetTripEnd(ChuyenXe trip)
        {
            var end = trip.NgayDi.Date.Add(trip.GioDenDuKien);

            if (trip.GioDenDuKien < trip.GioDi)
            {
                end = end.AddDays(1);
            }

            return end;
        }

        private static bool KhoangThoiGianGiaoNhau(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart < bEnd && aEnd > bStart;
        }
    }
}