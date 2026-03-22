using AdminDashboard.Models;
using AdminDashboard.TransportDBContext;
using AdminDashboard.Patterns.Command; 

namespace AdminDashboard.Patterns.Command 
{
    public class UpdateUserCommand : ICommand
    {
        private readonly Db27524Context _context;
        private readonly NguoiDung _user;
        private readonly NguoiDung _newData;

        private readonly string _oldName;
        private readonly string _oldEmail;

        public string LogMessage { get; private set; }

        public UpdateUserCommand(Db27524Context context, NguoiDung user, NguoiDung newData)
        {
            _context = context;
            _user = user;
            _newData = newData;

            _oldName = user.HoTen;
            _oldEmail = user.Email;
        }

        public void Execute()
        {
            _user.HoTen = _newData.HoTen;
            _user.Email = _newData.Email;
            _user.SoDienThoai = _newData.SoDienThoai;
            _user.NgaySinh = _newData.NgaySinh;

            _context.SaveChanges();

            LogMessage = $"Tên: {_oldName} → {_user.HoTen}, Email: {_oldEmail} → {_user.Email}";
        }

        public void Undo()
        {
            _user.HoTen = _oldName;
            _user.Email = _oldEmail;

            _context.SaveChanges();
        }
    }
}