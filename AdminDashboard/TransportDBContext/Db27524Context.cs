using AdminDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.TransportDBContext
{
    public class Db27524Context : DbContext
    {
        public Db27524Context(DbContextOptions<Db27524Context> options) : base(options) { }

        // DbSets
        public DbSet<Tram> Tram { get; set; }
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<VaiTro> VaiTro { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<LoaiXe> LoaiXe { get; set; }
        public DbSet<Xe> Xe { get; set; }
        public DbSet<LoTrinh> LoTrinh { get; set; }
        public DbSet<ChuyenXe> ChuyenXe { get; set; }
        public DbSet<Ghe> Ghe { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<Ve> Ve { get; set; }
        public DbSet<BaiViet> BaiViet { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<TaiXe> TaiXe { get; set; }
        public DbSet<ChuyenXeImage> ChuyenXeImage { get; set; }
        public DbSet<ChuyenXeImage> ChuyenXeImages { get; set; }
        public DbSet<ChuyenXe> ChuyenXes { get; set; }
        public virtual DbSet<TaiXe> TaiXes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính phức hợp cho bảng UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Cấu hình mối quan hệ nhiều-nhiều giữa NguoiDung và VaiTro
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.NguoiDung)          // Mỗi UserRole thuộc về một NguoiDung
                .WithMany(u => u.UserRoles)        // Mỗi NguoiDung có nhiều UserRole
                .HasForeignKey(ur => ur.UserId);   // Khóa ngoại là UserId

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.VaiTro)             // Mỗi UserRole thuộc về một VaiTro
                .WithMany(r => r.UserRoles)        // Mỗi VaiTro có nhiều UserRole
                .HasForeignKey(ur => ur.RoleId);   // Khóa ngoại là RoleId

            // Cấu hình mối quan hệ giữa LoTrinh và Tram 
            modelBuilder.Entity<LoTrinh>(entity =>
            {
                // Cấu hình mối quan hệ cho Trạm Đi
                entity.HasOne(l => l.TramDiNavigation)
                      .WithMany() 
                      .HasForeignKey(l => l.TramDi)
                      .OnDelete(DeleteBehavior.Restrict); 

                // Cấu hình mối quan hệ cho Trạm Tới
                entity.HasOne(l => l.TramToiNavigation)
                      .WithMany() 
                      .HasForeignKey(l => l.TramToi)
                      .OnDelete(DeleteBehavior.Restrict);
            });

       
            base.OnModelCreating(modelBuilder);

            // Khi xóa tài xế => xóa luôn người dùng liên quan
            modelBuilder.Entity<TaiXe>()
                .HasOne(tx => tx.NguoiDung)
                .WithMany()
                .HasForeignKey(tx => tx.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Nếu có liên kết khác (ví dụ Admin, LichPhanCong...) có thể thêm tương tự
        


        // 1. Cấu hình bảng Ve (Vé)
        modelBuilder.Entity<Ve>(entity =>
            {
             
                entity.HasOne(v => v.DonHang)
                    .WithMany() // Một Đơn Hàng có thể có nhiều Vé (nhưng không cần thuộc tính điều hướng ngược lại trong DonHang)
                    .HasForeignKey(v => v.DonHangId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Nếu xóa Ghế, KHÔNG xóa Vé. Hệ thống sẽ báo lỗi nếu bạn cố xóa Ghế đã có người đặt.
            
                entity.HasOne(v => v.Ghe)
                    .WithMany() // Một Ghế có thể thuộc về nhiều Vé (ở các chuyến khác nhau)
                    .HasForeignKey(v => v.GheID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. Cấu hình bảng DonHang (Đơn Hàng)
            modelBuilder.Entity<DonHang>(entity =>
            {
                // Quan hệ Đơn Hàng -> Người Dùng (Khách hàng)
                // Nếu xóa Người Dùng, xóa luôn các Đơn Hàng của họ.
                entity.HasOne(dh => dh.nguoiDung)
                    .WithMany()
                    .HasForeignKey(dh => dh.IDKhachHang)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ Đơn Hàng -> Chuyến Xe
                // Nếu xóa Chuyến Xe, KHÔNG cho xóa nếu vẫn còn Đơn Hàng.
           
                entity.HasOne(dh => dh.ChuyenXe)
                    .WithMany()
                    .HasForeignKey(dh => dh.ChuyenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });



        }

    }

    
}
