using AdminDashboard.Models;

namespace AdminDashboard.Patterns.Prototype
{
    public class ChuyenXeTemplate : IPrototype<ChuyenXePrototypeResult>
    {
        private readonly ChuyenXe _source;
        private readonly IReadOnlyCollection<ChuyenXeImage> _sourceImages;
        private readonly ChuyenXeCloneOptions _options;

        public ChuyenXeTemplate(
            ChuyenXe source,
            IEnumerable<ChuyenXeImage>? sourceImages,
            ChuyenXeCloneOptions options)
        {
            _source = source;
            _sourceImages = sourceImages?.ToList() ?? new List<ChuyenXeImage>();
            _options = options;
        }

        public ChuyenXePrototypeResult Clone()
        {
            var newChuyenId = Guid.NewGuid().ToString("N")[..8];

            var clonedTrip = new ChuyenXe
            {
                ChuyenId = newChuyenId,
                LoTrinhId = _source.LoTrinhId,
                XeId = _options.XeId,
                TaiXeId = _options.TaiXeId,
                NgayDi = _options.NgayDi.Date,
                GioDi = _options.GioDi,
                GioDenDuKien = _options.GioDenDuKien,
                TrangThai = _options.TrangThaiMacDinh
            };

            var clonedImages = new List<ChuyenXeImage>();

            if (_options.SaoChepHinhAnh)
            {
                clonedImages = _sourceImages
                    .Select(img => new ChuyenXeImage
                    {
                        ChuyenId = newChuyenId,
                        ImageUrl = img.ImageUrl,
                        CreatedAt = DateTime.Now
                    })
                    .ToList();
            }

            return new ChuyenXePrototypeResult(clonedTrip, clonedImages);
        }
    }
}