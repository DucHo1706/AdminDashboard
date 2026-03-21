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

