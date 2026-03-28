using AdminDashboard.Models;

namespace AdminDashboard.Patterns.ChainOfResponsibility
{
    public class LocTheoDiemDiHandler : TuyenXeSearchHandler
    {
        public override IQueryable<ChuyenXe> Handle(TimKiemRequest request)
        {
            if (!string.IsNullOrEmpty(request.DiemDi))
            {
                request.Query = request.Query.Where(c => c.LoTrinh.TramDi == request.DiemDi);
            }
            return base.Handle(request);
        }
    }

    public class LocTheoDiemDenHandler : TuyenXeSearchHandler
    {
        public override IQueryable<ChuyenXe> Handle(TimKiemRequest request)
        {
            if (!string.IsNullOrEmpty(request.DiemDen))
            {
                request.Query = request.Query.Where(c => c.LoTrinh.TramToi == request.DiemDen);
            }
            return base.Handle(request);
        }
    }

    public class LocTheoNgayDiHandler : TuyenXeSearchHandler
    {
        public override IQueryable<ChuyenXe> Handle(TimKiemRequest request)
        {
            if (!string.IsNullOrEmpty(request.NgayDi) && DateTime.TryParse(request.NgayDi, out DateTime parsedNgay))
            {
                request.Query = request.Query.Where(c => c.NgayDi.Date == parsedNgay.Date);
            }
            return base.Handle(request);
        }
    }
}