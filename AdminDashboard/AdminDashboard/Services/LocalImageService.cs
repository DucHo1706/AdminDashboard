using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AdminDashboard.Services
{
    public interface IImageService
{
    Task<List<string>> UploadImagesAsync(IFormFileCollection files);
    Task<bool> DeleteImageAsync(string imageUrl);
}
    public class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string ImageFolder = "images";

        public LocalImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<string>> UploadImagesAsync(IFormFileCollection files)
        {
            var imageUrls = new List<string>();
            if (files == null || files.Count == 0) return imageUrls;

            var uploadPath = Path.Combine(_environment.WebRootPath, ImageFolder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imageUrls.Add($"/{ImageFolder}/{fileName}");
                }
            }

            return imageUrls;
        }

        public Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}