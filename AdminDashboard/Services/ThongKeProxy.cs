using AdminDashboard.Areas.Admin.Components;
using AdminDashboard.Areas.Admin.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AdminDashboard.Services
{
    public class ThongKeProxy : IThongKeService
    {
        private readonly ThongKeService _realService;
        private readonly ILogger<ThongKeProxy> _logger;
        private readonly IMemoryCache _cache;

        public ThongKeProxy(
            ThongKeService realService,
            ILogger<ThongKeProxy> logger,
            IMemoryCache cache)
        {
            _realService = realService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ThongKeViewModel> LayThongKeAsync(ClaimsPrincipal user)
        {
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                _logger.LogWarning("Truy cập thống kê khi chưa đăng nhập.");
                throw new UnauthorizedAccessException("Bạn chưa đăng nhập.");
            }

            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("Người dùng {User} không có quyền xem thống kê admin.",
                    user.Identity?.Name);
                throw new UnauthorizedAccessException("Bạn không có quyền xem thống kê.");
            }

            const string cacheKey = "dashboard_thongke_admin";

            if (_cache.TryGetValue(cacheKey, out ThongKeViewModel cachedData))
            {
                _logger.LogInformation("Lấy thống kê từ cache.");
                return cachedData;
            }

            _logger.LogInformation("Lấy thống kê từ service thật.");
            var model = await _realService.LayThongKeAsync(user);

            _cache.Set(cacheKey, model, TimeSpan.FromSeconds(30));

            return model;
        }
    }
}