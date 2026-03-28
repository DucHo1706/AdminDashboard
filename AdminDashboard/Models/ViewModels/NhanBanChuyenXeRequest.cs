using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.ViewModels
{
    public class NhanBanChuyenXeRequest
    {
        [Required]
        public string SourceChuyenId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày đi là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime NgayDi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Giờ đi là bắt buộc")]
        public TimeSpan GioDi { get; set; }

        [Required(ErrorMessage = "Giờ đến dự kiến là bắt buộc")]
        public TimeSpan GioDenDuKien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn xe")]
        public string XeId { get; set; } = string.Empty;

        public string? TaiXeId { get; set; }

        public bool SaoChepHinhAnh { get; set; } = true;
        public bool SaoChepTaiXe { get; set; } = false;
        public bool ResetTrangThaiChoDuyet { get; set; } = true;
    }

    public class NhanBanChuyenXeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NewChuyenId { get; set; }
    }
}