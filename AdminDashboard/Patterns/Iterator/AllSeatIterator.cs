using System.Collections.Generic;
using System.Linq;

namespace AdminDashboard.Patterns.Iterator
{
    public class AllSeatIterator : BaseSeatIterator
    {
        public AllSeatIterator(IEnumerable<SeatDisplayItem> seats)
            : base(seats.OrderBy(s => int.TryParse(s.SoGhe, out var seatNo) ? seatNo : int.MaxValue))
        {
        }
    }
}