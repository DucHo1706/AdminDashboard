using System.Collections.Generic;
using System.Linq;

namespace AdminDashboard.Patterns.Iterator
{
    public class SoldSeatIterator : BaseSeatIterator
    {
        public SoldSeatIterator(IEnumerable<SeatDisplayItem> seats)
            : base(seats
                .Where(s => s.TrangThai == "DaBan")
                .OrderBy(s => int.TryParse(s.SoGhe, out var seatNo) ? seatNo : int.MaxValue))
        {
        }
    }
}