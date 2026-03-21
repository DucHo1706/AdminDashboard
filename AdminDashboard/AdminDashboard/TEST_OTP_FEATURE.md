# Hướng dẫn Test Tính năng OTP

## Tổng quan
Tính năng OTP (One-Time Password) đã được triển khai thành công với các thành phần sau:

### ✅ **Đã hoàn thành:**
1. **Models & Services**
   - `OtpCode` model - Lưu trữ thông tin OTP
   - `EmailService` - Gửi email với template đẹp
   - `OtpService` - Quản lý OTP (tạo, xác minh, cleanup)

2. **Controllers & Views**
   - `AuthController` - Thêm 3 action mới:
     - `ForgotPass` - Nhập email để gửi OTP
     - `VerifyOtp` - Nhập mã OTP 6 chữ số
     - `ResetPasswordWithOtp` - Đặt lại mật khẩu mới
   - **Views** - 3 trang đẹp với UI/UX hiện đại

3. **Database**
   - Bảng `OtpCodes` đã được tạo thành công
   - Có indexes để tối ưu hiệu suất

4. **Cấu hình**
   - Email settings đã được cấu hình trong `appsettings.json`
   - Services đã được đăng ký trong `Program.cs`

## Cách Test Tính năng:

### Bước 1: Khởi động ứng dụng
```bash
dotnet run
```
Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

### Bước 2: Test Flow OTP
1. **Truy cập trang Login**: `/Auth/Login`
2. **Click "Quên mật khẩu?"** ở cuối form
3. **Nhập email** của một user có trong database
4. **Click "Gửi yêu cầu"**
5. **Kiểm tra email** - sẽ nhận được mã OTP 6 chữ số
6. **Nhập mã OTP** vào trang VerifyOtp
7. **Đặt lại mật khẩu mới** trong trang ResetPasswordWithOtp

### Bước 3: Kiểm tra Database
```sql
-- Xem các OTP đã được tạo
SELECT * FROM OtpCodes ORDER BY CreatedAt DESC;

-- Xem OTP chưa hết hạn
SELECT * FROM OtpCodes WHERE ExpiresAt > GETDATE() AND IsUsed = 0;
```

## Cấu hình Email hiện tại:
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "vokhuong14805@gmail.com",
    "SmtpPassword": "ubvx fant muuv dmkl",
    "FromEmail": "GoSix@gmail.com"
  }
}
```

## Tính năng chính:
1. **Bảo mật cao**: OTP có thời hạn 10 phút
2. **UI/UX đẹp**: Giao diện hiện đại, responsive
3. **Validation đầy đủ**: Kiểm tra email, OTP, mật khẩu
4. **Error handling**: Xử lý lỗi tốt với thông báo rõ ràng
5. **Cleanup tự động**: Xóa OTP hết hạn

## Troubleshooting:
- **Lỗi gửi email**: Kiểm tra cấu hình SMTP
- **OTP không hợp lệ**: Kiểm tra thời gian hết hạn
- **Email không tồn tại**: Kiểm tra user trong database

## Lưu ý:
- OTP có thời hạn 10 phút
- Mỗi email chỉ có thể có 1 OTP hợp lệ tại một thời điểm
- OTP sẽ tự động bị xóa sau khi sử dụng hoặc hết hạn

