using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;

namespace AdminDashboard.Patterns.Command
{
    public class UpdateUserCommand : ICommand
    {
        private readonly Db27524Context _context;
        private readonly NguoiDung _user;
        private readonly NguoiDung _model;

        public UpdateUserCommand(Db27524Context context, NguoiDung user, NguoiDung model)
        {
            _context = context;
            _user = user;
            _model = model;
        }

        public void Execute()
        {
            _user.HoTen = _model.HoTen;
            _user.Email = _model.Email;
            _user.SoDienThoai = _model.SoDienThoai;
            _user.NgaySinh = _model.NgaySinh;

            _context.SaveChanges();
        }
    }
}
