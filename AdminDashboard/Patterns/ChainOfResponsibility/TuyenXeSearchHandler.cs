using AdminDashboard.Models;

namespace AdminDashboard.Patterns.ChainOfResponsibility
{
    public abstract class TuyenXeSearchHandler
    {
        protected TuyenXeSearchHandler _nextHandler;

        public TuyenXeSearchHandler SetNext(TuyenXeSearchHandler nextHandler)
        {
            _nextHandler = nextHandler;
            return nextHandler;
        }

        public virtual IQueryable<ChuyenXe> Handle(TimKiemRequest request)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.Handle(request);
            }
            return request.Query;
        }
    }
}