namespace AdminDashboard.Patterns.Observer
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Notify(string message);
    }
}
