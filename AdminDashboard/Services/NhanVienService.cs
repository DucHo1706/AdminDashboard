using AdminDashboard.Models;
using AdminDashboard.Models.ViewModels;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly Db27524Context _context;
        private readonly IImageService _imageService;

        public NhanVienService(Db27524Context context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<string> TaoNhanVienAsync(TaoNhanVienRequest req, string nhaXeId)
        {
            var existingUser = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (existingUser != null) return "Email này đã được sử dụng.";
            string avatarUrl = null;
            if (req.Avatar != null)
            {
                var files = new FormFileCollection { req.Avatar };
                var urls = await _imageService.UploadImagesAsync(files);
                if (urls != null && urls.Any()) avatarUrl = urls[0];
            }
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newUserId = Guid.NewGuid().ToString();
                    var newUser = new NguoiDung
                    {
                        UserId = newUserId,
                        HoTen = req.HoTen,
                        Email = req.Email,
                        SoDienThoai = req.SoDienThoai,
                        MatKhau = req.MatKhau,
                        TrangThai = TrangThaiNguoiDung.HoatDong, 
                        NhaXeId = nhaXeId,
                        NgaySinh = DateTime.Now
                    };
                    _context.NguoiDung.Add(newUser);

                    string roleName = req.VaiTro == VaiTroNhanVien.TaiXe ? "TaiXe" : "NhanVienBanVe";

                    var role = await _context.VaiTro.FirstOrDefaultAsync(r => r.TenVaiTro == roleName);
                    if (role == null)
                    {
                        role = new VaiTro { RoleId = Guid.NewGuid().ToString(), TenVaiTro = roleName };
                        _context.VaiTro.Add(role);
                        await _context.SaveChangesAsync();
                    }

                    _context.UserRole.Add(new UserRole { UserId = newUserId, RoleId = role.RoleId });

                    var nv = new NhanVien
                    {
                        NhanVienId = Guid.NewGuid().ToString(),
                        HoTen = req.HoTen,
                        SoDienThoai = req.SoDienThoai,
                        CCCD = req.CCCD,
                        SoBangLai = req.SoBangLai,
                        HangBangLai = req.HangBangLai,
                        VaiTro = req.VaiTro,
                        AvatarUrl = avatarUrl,
                        NhaXeId = nhaXeId,
                        AccountId = newUserId,
                        NgayVaoLam = DateTime.Now,
                        DangLamViec = true
                    };
                    _context.NhanVien.Add(nv);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return "Success";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return "Lỗi hệ thống: " + ex.Message;
                }
            });
        }
    }
}