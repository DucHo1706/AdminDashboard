using System.Collections.Generic;
using System.Linq;

namespace AdminDashboard.Patterns.Iterator
{
    public class SeatCollection : ISeatCollection
    {
        private readonly List<SeatDisplayItem> _seats;

        public SeatCollection(IEnumerable<SeatDisplayItem> seats)
        {
            _seats = seats
                .OrderBy(s => int.TryParse(s.SoGhe, out var seatNo) ? seatNo : int.MaxValue)
                .ToList();
        }

        public int Count => _seats.Count;

        public ISeatIterator CreateAllIterator()
        {
            return new AllSeatIterator(_seats);
        }

        public ISeatIterator CreateAvailableIterator()
        {
            return new AvailableSeatIterator(_seats);
        }

        public ISeatIterator CreateSoldIterator()
        {
            return new SoldSeatIterator(_seats);
        }
    }
}