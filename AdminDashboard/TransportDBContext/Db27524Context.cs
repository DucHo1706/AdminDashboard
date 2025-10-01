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
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<LoaiXe> LoaiXe { get; set; }
        public DbSet<Xe> Xe { get; set; }
        public DbSet<LoTrinh> LoTrinh { get; set; }
        public DbSet<ChuyenXe> ChuyenXe { get; set; }
        public DbSet<Ghe> Ghe { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<Ve> Ve { get; set; }
        public DbSet<BaiViet> BaiViet { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public object LoTrinhId { get; internal set; }
        public object XeId { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties 
            
             */
            //   Tram  
            modelBuilder.Entity<Tram>()
                .HasKey(t => t.IdTram);

            //   NguoiDung  
            modelBuilder.Entity<NguoiDung>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<NguoiDung>()
                .HasIndex(u => u.TenDangNhap).IsUnique();

            modelBuilder.Entity<NguoiDung>()
                .HasIndex(u => u.Email).IsUnique();

            //   VaiTro  
            modelBuilder.Entity<VaiTro>()
                .HasKey(r => r.RoleId);

            modelBuilder.Entity<VaiTro>()
                .HasIndex(r => r.TenVaiTro).IsUnique();

            //   UserRole  
            modelBuilder.Entity<UserRole>()
          .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.NguoiDung)
                .WithMany(nd => nd.UserRoles) // nếu có collection, không có thì .WithMany()
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.VaiTro)
                .WithMany(vt => vt.UserRoles) // nếu có collection, không có thì .WithMany()
                .HasForeignKey(ur => ur.RoleId);


            //   KhachHang  
            modelBuilder.Entity<KhachHang>()
                .HasKey(kh => kh.IDKhachHang);

            modelBuilder.Entity<KhachHang>()
                .HasIndex(kh => kh.DiaChiMail).IsUnique();

            modelBuilder.Entity<KhachHang>()
                .HasIndex(kh => kh.UserId).IsUnique();

            //   LoaiXe  
            modelBuilder.Entity<LoaiXe>()
                .HasKey(lx => lx.LoaiXeId);

            //   Xe  
            modelBuilder.Entity<Xe>()
                .HasKey(x => x.XeId);

            modelBuilder.Entity<Xe>()
                .HasIndex(x => x.BienSoXe).IsUnique();

            modelBuilder.Entity<Xe>()
                  .HasOne(x => x.LoaiXe)        // dùng property trong class Xe
                  .WithMany()                   // 1 LoaiXe có nhiều Xe
                  .HasForeignKey(x => x.LoaiXeId)
                  .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<LoTrinh>(entity =>
            {
                entity.HasKey(lt => lt.LoTrinhId);

                entity.Property(lt => lt.LoTrinhId)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(lt => lt.TramDi)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(lt => lt.TramToi)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(lt => lt.GiaVeCoDinh)
                      .HasColumnType("numeric(10,2)");

                // Quan hệ TramDi
                entity.HasOne(lt => lt.TramDiNavigation)
                      .WithMany()
                      .HasForeignKey(lt => lt.TramDi)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ TramToi
                entity.HasOne(lt => lt.TramToiNavigation)
                      .WithMany()
                      .HasForeignKey(lt => lt.TramToi)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(lt => new { lt.TramDi, lt.TramToi });
            });

            //   ChuyenXe  
            modelBuilder.Entity<ChuyenXe>(entity =>
            {
                entity.HasKey(cx => cx.ChuyenId);

                entity.Property(cx => cx.ChuyenId)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(cx => cx.LoTrinhId)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(cx => cx.XeId)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(cx => cx.TrangThai)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(cx => cx.NgayDi)
                      .IsRequired();

                entity.Property(cx => cx.GioDi)
                      .IsRequired();

                entity.Property(cx => cx.GioDenDuKien)
                      .IsRequired();

                entity.HasOne(cx => cx.LoTrinh)
                      .WithMany()
                      .HasForeignKey(cx => cx.LoTrinhId);

                entity.HasOne(cx => cx.Xe)
                      .WithMany()
                      .HasForeignKey(cx => cx.XeId);

                entity.HasIndex(cx => cx.NgayDi);
            });

            //   Ghe  
            modelBuilder.Entity<Ghe>()
                .HasKey(g => g.GheID);

            modelBuilder.Entity<Ghe>()
                .HasOne<Xe>()
                .WithMany()
                .HasForeignKey(g => g.XeId);

            modelBuilder.Entity<Ghe>()
                .HasIndex(g => new { g.XeId, g.SoGhe }).IsUnique();

            //   DonHang  
            modelBuilder.Entity<DonHang>()
                .HasKey(dh => dh.DonHangId);

            modelBuilder.Entity<DonHang>()
                .HasOne<KhachHang>()
                .WithMany()
                .HasForeignKey(dh => dh.IDKhachHang);

            modelBuilder.Entity<DonHang>()
                .HasOne<ChuyenXe>()
                .WithMany()
                .HasForeignKey(dh => dh.ChuyenId);

            modelBuilder.Entity<DonHang>()
                .HasIndex(dh => dh.IDKhachHang);

            //   Ve  
            modelBuilder.Entity<Ve>()
                .HasKey(v => v.VeId);

            modelBuilder.Entity<Ve>()
                .HasOne<DonHang>()
                .WithMany()
                .HasForeignKey(v => v.DonHangId);

            modelBuilder.Entity<Ve>()
                .HasOne<Ghe>()
                .WithMany()
                .HasForeignKey(v => v.GheID);

            modelBuilder.Entity<Ve>()
                .HasIndex(v => new { v.DonHangId, v.GheID }).IsUnique();

            modelBuilder.Entity<Ve>()
                .HasIndex(v => v.GheID);

            //   BaiViet  
            modelBuilder.Entity<BaiViet>()
                .HasKey(bv => bv.Id);

            modelBuilder.Entity<BaiViet>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(bv => bv.AdminId);

            //   Menu  
            modelBuilder.Entity<Menu>()
                .HasKey(m => m.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
