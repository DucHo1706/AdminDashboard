using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public enum VaiTroNhanVien
    {
        TaiXe = 1,
        NhanVienBanVe = 2
    }

    public class NhanVien
    {
        [Key]
        [StringLength(255)] // Sửa theo yêu cầu của bạn
        public string NhanVienId { get; set; } // Dùng string thay vì int

        [Required(ErrorMessage = "Phải nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Phải nhập SĐT")]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        public string? CCCD { get; set; }

        public string? SoBangLai { get; set; }
        public string? HangBangLai { get; set; }

        public string? AvatarUrl { get; set; }

        public VaiTroNhanVien VaiTro { get; set; }

        public bool DangLamViec { get; set; } = true;
        public DateTime NgayVaoLam { get; set; } = DateTime.Now;

        // LIÊN KẾT NHÀ XE
        public string NhaXeId { get; set; }
        [ForeignKey("NhaXeId")]
        public virtual NhaXe? NhaXe { get; set; }

        // LIÊN KẾT USER
        public string? AccountId { get; set; }
    }
}