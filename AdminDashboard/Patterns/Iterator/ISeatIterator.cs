namespace AdminDashboard.Patterns.Iterator
{
    public interface ISeatIterator
    {
        bool HasNext();
        SeatDisplayItem Next();
        void Reset();
    }
}