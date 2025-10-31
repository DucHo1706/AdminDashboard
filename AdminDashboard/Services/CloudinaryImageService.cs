using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;

namespace AdminDashboard.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageService> _logger;

        public CloudinaryImageService(Cloudinary cloudinary, ILogger<CloudinaryImageService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<List<string>> UploadImagesAsync(IFormFileCollection files)
        {
            var imageUrls = new List<string>();

            if (files == null || files.Count == 0)
                return imageUrls;

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        throw new InvalidOperationException("Định dạng file không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG, GIF, WEBP");
                    }

                    if (file.Length > 5 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");
                    }

                    try
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(file.FileName, stream),
                                Folder = "chuyenxe_images",
                                Transformation = new Transformation()
                                    .Quality("auto")
                                    .FetchFormat("auto")
                                    .Width(1200)
                                    .Height(800)
                                    .Crop("limit")
                            };

                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                imageUrls.Add(uploadResult.SecureUrl.ToString());
                                _logger.LogInformation($"✅ Đã upload ảnh: {uploadResult.SecureUrl}");
                            }
                            else
                            {
                                _logger.LogError($"❌ Upload thất bại: {uploadResult.Error?.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Lỗi upload ảnh {file.FileName}");
                        throw new Exception($"Lỗi khi upload ảnh {file.FileName}: {ex.Message}");
                    }
                }
            }

            return imageUrls;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var publicId = GetImagePublicId(imageUrl);
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result == "ok")
                {
                    _logger.LogInformation($"✅ Đã xóa ảnh: {publicId}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"⚠️ Không thể xóa ảnh: {result.Result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Lỗi khi xóa ảnh: {imageUrl}");
                return false;
            }
        }

        public string GetImagePublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                // Lấy tên file không có phần mở rộng (ví dụ: "abcxyz123")
                var fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                return $"chuyenxe_images/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Không thể phân tích URL để lấy Public ID: {imageUrl}. Thử fallback.");

                // Fallback nếu URL không hợp lệ (lấy phần cuối cùng)
                var fileName = Path.GetFileNameWithoutExtension(imageUrl);
                return $"chuyenxe_images/{fileName}";
            }
        }
    }
}