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
     
      
        public DbSet<ChuyenXeImage> ChuyenXeImage { get; set; }
        public DbSet<ChuyenXe> ChuyenXes { get; set; }
        public virtual DbSet<NhaXe> NhaXe { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // các role mặc định ban đầu 
            // 1. Định nghĩa danh sách các Role cần tạo

            var adminId = "b9f3d6a1-5c8e-4a7d-9b2c-1e3f4a5d6c7b";
            var chuNhaXeId = "c8e2f1a0-4d9b-3a6c-8e1f-0d2e3b4a5c6d";
            var nhanVienId = "d7a1e0b9-3c8f-2a5e-7d0b-9c1f2e3d4a5b";
            var taiXeId = "e6c0d9a8-2b7e-1a4d-6c9a-8b0f1e2d3c4a";
            var khachHangId = "f5b9c8a7-1a6d-0e3c-5b8a-7a9f0e1d2c3b";

            // Mồi dữ liệu với ID tĩnh
            modelBuilder.Entity<VaiTro>().HasData(
                new VaiTro { RoleId = adminId, TenVaiTro = "Admin" },
                new VaiTro { RoleId = chuNhaXeId, TenVaiTro = "ChuNhaXe" },
                new VaiTro { RoleId = nhanVienId, TenVaiTro = "NhanVien" },
                new VaiTro { RoleId = taiXeId, TenVaiTro = "TaiXe" },
                new VaiTro { RoleId = khachHangId, TenVaiTro = "KhachHang" }
            );

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
