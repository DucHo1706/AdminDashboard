using System.Collections.Generic;

namespace AdminDashboard.Patterns.Observer
{
    public class DashboardService : ISubject
    {
        private List<IObserver> observers = new List<IObserver>();

        public void AddObserver(IObserver observer)
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