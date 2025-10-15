using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminDashboard.Models
{
    public enum TrangThaiNguoiDung
    {
        HoatDong,
        BiKhoa
<<<<<<< HEAD
       
    }
=======
    }

>>>>>>> origin/ThanhToanMuaVe
    public class NguoiDung
    {
        [Key]
        [StringLength(255)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required]
<<<<<<< HEAD
        [StringLength(255)] 
        public string MatKhau { get; set; }

        [StringLength(10)] 
=======
        [StringLength(255)]
        public string MatKhau { get; set; }

        [StringLength(10)]
>>>>>>> origin/ThanhToanMuaVe
        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [Required]
<<<<<<< HEAD
        public TrangThaiNguoiDung TrangThai { get; set; } = TrangThaiNguoiDung.HoatDong; // Sử dụng Enum

        // Mối quan hệ nhiều-nhiều với VaiTro thông qua bảng UserRole
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
=======
        public TrangThaiNguoiDung TrangThai { get; set; } = TrangThaiNguoiDung.HoatDong;

         public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
>>>>>>> origin/ThanhToanMuaVe
