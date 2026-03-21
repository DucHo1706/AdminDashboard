using System.Collections.Generic;

namespace AdminDashboard.Patterns.Iterator
{
    public abstract class BaseSeatIterator : ISeatIterator
    {
        private readonly List<SeatDisplayItem> _items;
        private int _currentIndex;

        protected BaseSeatIterator(IEnumerable<SeatDisplayItem> items)
        {
            _items = items.ToList();
            _currentIndex = 0;
        }

        public bool HasNext()
        {
            return _currentIndex < _items.Count;
        }

        public SeatDisplayItem Next()
        {
            if (!HasNext())
            {
                throw new InvalidOperationException("Không còn ghế nào để duyệt.");
            }

            return _items[_currentIndex++];
        }

        public void Reset()
        {
            _currentIndex = 0;
        }
    }
}