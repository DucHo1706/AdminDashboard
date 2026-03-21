namespace AdminDashboard.Patterns.Iterator
{
    public interface ISeatCollection
    {
        ISeatIterator CreateAllIterator();
        ISeatIterator CreateAvailableIterator();
        ISeatIterator CreateSoldIterator();
        int Count { get; }
    }
}