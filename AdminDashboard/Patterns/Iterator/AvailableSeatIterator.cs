using System.Collections.Generic;
using System.Linq;

namespace AdminDashboard.Patterns.Iterator
{
    public class AvailableSeatIterator : BaseSeatIterator
    {
        public AvailableSeatIterator(IEnumerable<SeatDisplayItem> seats)
            : base(seats
                .Where(s => s.TrangThai == "Trong")
                .OrderBy(s => int.TryParse(s.SoGhe, out var seatNo) ? seatNo : int.MaxValue))
        {
        }
    }
}