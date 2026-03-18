namespace AdminDashboard.Patterns.Observer
{
    public class DashboardService : ISubject
    {
        private List<IObserver> observers = new List<IObserver>();

        public void Attach(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Notify(string message)
        {
            foreach (var o in observers)
            {
                o.Update(message);
            }
        }
    }
}
