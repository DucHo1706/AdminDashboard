    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace AdminDashboard.Models
    {
        [Table("TaiXe")]
        public class TaiXe
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int TaiXeId { get; set; }
            //[Key]
            [StringLength(255)]
            public string UserId { get; set; }

            [Required]
            [StringLength(450)]
            public string AdminId { get; set; }


            [Required]
            [StringLength(50)]
            public string BangLaiXe { get; set; }

            [Required]
            public DateTime NgayVaoLam { get; set; }

            [Required]
            [StringLength(50)]
            public string TrangThai { get; set; } = "Hoạt động";

            [ForeignKey("UserId")]
            public virtual NguoiDung NguoiDung { get; set; }

            [ForeignKey("AdminId")]
            public virtual NguoiDung Admin { get; set; }
            [Required]
            [StringLength(100)]
            public string HoTen { get; set; }
        }
    }