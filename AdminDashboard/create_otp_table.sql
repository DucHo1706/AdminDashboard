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
