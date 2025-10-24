# Hướng dẫn triển khai tính năng OTP

## Tổng quan
Tính năng OTP (One-Time Password) đã được triển khai thành công với các thành phần sau:

### 1. Models đã tạo:
- `OtpCode.cs` - Model cho bảng OTP
- `ForgotPasswordModels.cs` - Models cho quá trình đặt lại mật khẩu

### 2. Services đã tạo:
- `IEmailService` và `EmailService` - Dịch vụ gửi email
- `IOtpService` và `OtpService` - Dịch vụ quản lý OTP

### 3. Controllers đã cập nhật:
- `AuthController` - Thêm các action: ForgotPass, VerifyOtp, ResetPasswordWithOtp

### 4. Views đã tạo:
- `ForgotPass.cshtml` - Trang nhập email để gửi OTP
- `VerifyOtp.cshtml` - Trang nhập mã OTP
- `ResetPasswordWithOtp.cshtml` - Trang đặt lại mật khẩu mới

### 5. Cấu hình đã cập nhật:
- `Program.cs` - Đăng ký các dịch vụ
- `appsettings.json` - Thêm cấu hình email
- `Db27524Context.cs` - Thêm DbSet cho OtpCode

## Cách triển khai:

### Bước 1: Tạo bảng OtpCode trong database
Chạy script SQL trong file `create_otp_table.sql`:

```sql
-- Script để tạo bảng OtpCode
CREATE TABLE [dbo].[OtpCodes] (
    [Id] nvarchar(450) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Code] nvarchar(6) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [Purpose] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_OtpCodes] PRIMARY KEY ([Id])
);

-- Tạo index cho Email để tìm kiếm nhanh hơn
CREATE INDEX [IX_OtpCodes_Email] ON [OtpCodes] ([Email]);

-- Tạo index cho ExpiresAt để cleanup nhanh hơn
CREATE INDEX [IX_OtpCodes_ExpiresAt] ON [OtpCodes] ([ExpiresAt]);
```

### Bước 2: Cấu hình email
Cập nhật file `appsettings.json` với thông tin email thực tế:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",
  "FromEmail": "your-email@gmail.com"
}
```

**Lưu ý:** Để sử dụng Gmail, bạn cần:
1. Bật 2-Factor Authentication
2. Tạo App Password
3. Sử dụng App Password thay vì mật khẩu thường

### Bước 3: Test tính năng
1. Chạy ứng dụng
2. Truy cập trang đăng nhập
3. Click "Quên mật khẩu?"
4. Nhập email có trong hệ thống
5. Kiểm tra email để lấy mã OTP
6. Nhập mã OTP
7. Đặt lại mật khẩu mới

## Tính năng chính:

### 1. Gửi OTP qua email
- Tạo mã OTP 6 chữ số ngẫu nhiên
- Gửi email với template đẹp
- OTP có hiệu lực 10 phút
- Chỉ sử dụng được một lần

### 2. Xác minh OTP
- Kiểm tra mã OTP hợp lệ
- Kiểm tra thời gian hết hạn
- Đánh dấu OTP đã sử dụng

### 3. Đặt lại mật khẩu
- Xác minh lại OTP
- Kiểm tra độ mạnh mật khẩu
- Cập nhật mật khẩu mới

### 4. Bảo mật
- OTP tự động hết hạn sau 10 phút
- Mỗi OTP chỉ sử dụng một lần
- Xóa OTP cũ khi tạo mới
- Cleanup tự động OTP hết hạn

## Troubleshooting:

### Lỗi gửi email:
1. Kiểm tra cấu hình SMTP
2. Kiểm tra App Password (nếu dùng Gmail)
3. Kiểm tra firewall/antivirus

### Lỗi database:
1. Đảm bảo đã chạy script tạo bảng
2. Kiểm tra connection string
3. Kiểm tra quyền truy cập database

### OTP không hoạt động:
1. Kiểm tra email có trong hệ thống
2. Kiểm tra OTP chưa hết hạn
3. Kiểm tra OTP chưa được sử dụng

