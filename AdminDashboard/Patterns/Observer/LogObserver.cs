using System;

namespace AdminDashboard.Patterns.Observer
{
    public class LogObserver : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine("👉 Observer: " + message);
        }
    }
}