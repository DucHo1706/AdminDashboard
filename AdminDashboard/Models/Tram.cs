using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AdminDashboard.Models
{
    public class Tram
    {
        [Key]
        [StringLength(255)]
        [Display(Name = "Mã Trạm")]
        public string IdTram { get; set; }

        [Required(ErrorMessage = "Tên trạm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên trạm không được vượt quá 100 ký tự")]
        [Display(Name = "Tên Trạm")]
        public string TenTram { get; set; }

        [Required(ErrorMessage = "Địa chỉ trạm là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa Chỉ")]
        public string DiaChiTram { get; set; }
        public string Tinh { get; set; }
        public string Huyen { get; set; }
        public string Xa { get; set; }
    }
}