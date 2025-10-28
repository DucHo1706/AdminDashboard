# Hướng dẫn cấu hình Email cho tính năng OTP

## Cấu hình Email Settings

Để sử dụng tính năng gửi mã OTP qua email, bạn cần cấu hình thông tin email trong file `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Hệ thống quản lý vận tải"
  }
}
```

## Cấu hình Gmail

### Bước 1: Bật 2-Factor Authentication
1. Đăng nhập vào tài khoản Gmail của bạn
2. Vào **Settings** > **Security**
3. Bật **2-Step Verification**

### Bước 2: Tạo App Password
1. Trong phần **Security**, tìm **App passwords**
2. Chọn **Mail** và **Other (Custom name)**
3. Nhập tên ứng dụng (ví dụ: "Admin Dashboard")
4. Copy mật khẩu được tạo (16 ký tự)

### Bước 3: Cập nhật appsettings.json
Thay thế các giá trị sau:
- `your-email@gmail.com`: Email Gmail của bạn
- `your-app-password`: Mật khẩu ứng dụng từ bước 2

## Các nhà cung cấp email khác

### Outlook/Hotmail
```json
{
  "SmtpHost": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@outlook.com",
  "SmtpPassword": "your-password"
}
```

### Yahoo Mail
```json
{
  "SmtpHost": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@yahoo.com",
  "SmtpPassword": "your-app-password"
}
```

## Tính năng đã được triển khai

✅ **Model OtpCode**: Lưu trữ mã OTP với thời gian hết hạn
✅ **EmailService**: Gửi email OTP với template đẹp
✅ **AuthController**: Xử lý logic OTP và reset password
✅ **Views**: Giao diện người dùng cho từng bước
✅ **Database**: Bảng OtpCode đã được tạo

## Quy trình hoạt động

1. **Nhập email**: Người dùng nhập email trên trang `/Auth/ForgotPassword`
2. **Gửi OTP**: Hệ thống tạo mã OTP 6 chữ số và gửi qua email
3. **Xác thực OTP**: Người dùng nhập mã OTP trên trang `/Auth/VerifyOtp`
4. **Đặt lại mật khẩu**: Sau khi xác thực thành công, chuyển đến trang `/Auth/ResetPasswordWithOtp`

## Bảo mật

- Mã OTP có hiệu lực 10 phút
- Mỗi mã chỉ sử dụng được 1 lần
- Phiên đặt lại mật khẩu có hiệu lực 30 phút sau khi xác thực OTP
- Email template có thiết kế chuyên nghiệp với thông tin bảo mật

## Kiểm tra hoạt động

Sau khi cấu hình email, bạn có thể test tính năng bằng cách:
1. Truy cập `/Auth/ForgotPassword`
2. Nhập email có trong hệ thống
3. Kiểm tra email để nhận mã OTP
4. Hoàn thành quy trình đặt lại mật khẩu
