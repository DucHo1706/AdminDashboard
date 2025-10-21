using AdminDashboard.Models.TrangThai;
using AdminDashboard.TransportDBContext;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Services
{
    public class TrangThaiChuyenXeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TrangThaiChuyenXeService> _logger;

        public TrangThaiChuyenXeService(IServiceProvider serviceProvider, ILogger<TrangThaiChuyenXeService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dich vu cap nhat trang thai chuyen xe dang chay.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<Db27524Context>();

                        var now = DateTime.Now;
                        var thoiGianNgungBanVe = now.AddMinutes(30);                
                        var chuyenXeCanCapNhat = await context.ChuyenXe
                            .Where(c => c.TrangThai == TrangThaiChuyenXe.DangMoBanVe && // Chỉ cập nhật các chuyến đang mở bán vé
                                        (c.NgayDi.Date < thoiGianNgungBanVe.Date || // Ngày đi trước thời gian ngừng bán vé
                                         (c.NgayDi.Date == thoiGianNgungBanVe.Date && c.GioDi <= thoiGianNgungBanVe.TimeOfDay))) // Hoặc cùng ngày và giờ đi trước hoặc bằng thời gian ngừng bán vé
                            .ToListAsync(stoppingToken);

                        if (chuyenXeCanCapNhat.Any())
                        {
                            foreach (var chuyen in chuyenXeCanCapNhat)
                            {
                                chuyen.TrangThai = TrangThaiChuyenXe.ChoKhoiHanh;
                                _logger.LogInformation($"Chuyen xe ID {chuyen.ChuyenId} da duoc cap nhat sang trang thai ChoKhoiHanh.");
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Loi xay ra khi cap nhat trang thai chuyen xe.");
                }

                // Chờ 1 phút trước khi kiểm tra lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}