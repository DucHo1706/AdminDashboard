namespace AdminDashboard.Patterns.Observer
{
    public interface ISubject
    {
        void AddObserver(IObserver observer);
        void Notify(string message);
    }
}