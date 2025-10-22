# Hướng dẫn sử dụng tính năng Quên mật khẩu với OTP

## Tổng quan
Hệ thống quên mật khẩu với OTP đã được implement hoàn chỉnh với các tính năng sau:

1. **Gửi OTP qua email**: Khách hàng nhập email để nhận mã OTP
2. **Xác thực OTP**: Nhập mã OTP để xác thực danh tính
3. **Đặt lại mật khẩu**: Tạo mật khẩu mới sau khi xác thực thành công

## Cấu hình Email

### 1. Cập nhật appsettings.json
```json
{
  "EmailSettings": {
    "Email": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### 2. Tạo App Password cho Gmail
1. Đăng nhập vào Gmail
2. Vào Settings > Security > 2-Step Verification
3. Tạo App Password cho ứng dụng
4. Sử dụng App Password thay vì mật khẩu thường

## Cài đặt Database

### Chạy SQL script để tạo bảng OTP:
```sql
-- Tạo bảng OtpVerification
CREATE TABLE [dbo].[OtpVerification] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [OtpCode] nvarchar(6) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL DEFAULT 0,
    [UserId] nvarchar(255) NULL,
    CONSTRAINT [PK_OtpVerification] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OtpVerification_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([UserId])
);

-- Tạo index cho UserId
CREATE INDEX [IX_OtpVerification_UserId] ON [OtpVerification] ([UserId]);
```

## Luồng hoạt động

### 1. Quên mật khẩu
- **URL**: `/Auth/ForgotPass`
- **Action**: Khách hàng nhập email
- **Xử lý**: 
  - Kiểm tra email có tồn tại trong hệ thống
  - Tạo mã OTP 6 chữ số ngẫu nhiên
  - Lưu OTP vào database (có hiệu lực 10 phút)
  - Gửi email OTP đến khách hàng
  - Chuyển đến trang xác thực OTP

### 2. Xác thực OTP
- **URL**: `/Auth/VerifyOtp`
- **Action**: Khách hàng nhập mã OTP
- **Xử lý**:
  - Kiểm tra mã OTP có hợp lệ và chưa hết hạn
  - Đánh dấu OTP đã được sử dụng
  - Chuyển đến trang đặt lại mật khẩu

### 3. Đặt lại mật khẩu
- **URL**: `/Auth/SetNewPassword`
- **Action**: Khách hàng nhập mật khẩu mới
- **Xử lý**:
  - Kiểm tra mật khẩu mới và xác nhận
  - Cập nhật mật khẩu trong database
  - Xóa tất cả OTP của email này
  - Chuyển về trang đăng nhập với thông báo thành công

## Tính năng bổ sung

### 1. Gửi lại OTP
- Khách hàng có thể yêu cầu gửi lại mã OTP
- OTP cũ sẽ bị xóa và tạo mã mới
- Thời gian hiệu lực được reset về 10 phút

### 2. Bảo mật
- OTP chỉ có hiệu lực 10 phút
- Mỗi OTP chỉ được sử dụng 1 lần
- Tự động xóa OTP cũ khi tạo mới
- Kiểm tra độ mạnh của mật khẩu mới

### 3. Giao diện người dùng
- Thiết kế responsive và thân thiện
- Hiển thị countdown timer cho OTP
- Validation real-time cho mật khẩu
- Thông báo lỗi và thành công rõ ràng

## API Endpoints

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/Auth/ForgotPass` | Trang quên mật khẩu |
| POST | `/Auth/ForgotPass` | Xử lý gửi OTP |
| GET | `/Auth/VerifyOtp` | Trang nhập OTP |
| POST | `/Auth/VerifyOtp` | Xác thực OTP |
| GET | `/Auth/SetNewPassword` | Trang đặt lại mật khẩu |
| POST | `/Auth/SetNewPassword` | Cập nhật mật khẩu mới |
| POST | `/Auth/ResendOtp` | Gửi lại OTP (AJAX) |

## Lưu ý quan trọng

1. **Cấu hình Email**: Phải cấu hình đúng email và app password trong appsettings.json
2. **Database**: Phải tạo bảng OtpVerification trước khi sử dụng
3. **Bảo mật**: OTP có thời hạn và chỉ sử dụng 1 lần
4. **Testing**: Test với email thật để đảm bảo tính năng hoạt động

## Troubleshooting

### Lỗi gửi email
- Kiểm tra cấu hình EmailSettings trong appsettings.json
- Đảm bảo đã bật 2-Step Verification và tạo App Password
- Kiểm tra log để xem chi tiết lỗi

### Lỗi database
- Đảm bảo đã tạo bảng OtpVerification
- Kiểm tra connection string
- Xem log Entity Framework để debug

### OTP không hợp lệ
- Kiểm tra thời gian hết hạn (10 phút)
- Đảm bảo OTP chưa được sử dụng
- Kiểm tra format mã OTP (6 chữ số)
