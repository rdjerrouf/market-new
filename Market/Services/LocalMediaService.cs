using System;
using System.IO;
using System.Threading.Tasks;

namespace Market.Services
{
    public class LocalMediaService : IMediaService
    {
        private readonly string _imageDirectory;

        public LocalMediaService()
        {
            // Use app data directory for local storage
            _imageDirectory = Path.Combine(FileSystem.AppDataDirectory, "Images");

            // Make sure the directory exists
            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);
        }

        public async Task<string> UploadImageAsync(FileResult file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using var stream = await file.OpenReadAsync();
            return await UploadImageAsync(stream, file.FileName);
        }

        public async Task<string> UploadImageAsync(Stream stream, string fileName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            // Create a unique file name
            string fileExtension = Path.GetExtension(fileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(_imageDirectory, uniqueFileName);

            // Save the file
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Return the relative path
            return $"Images/{uniqueFileName}";
        }

        public Task<bool> DeleteImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return Task.FromResult(false);

            try
            {
                // Extract the file name from the URL
                string fileName = Path.GetFileName(url);
                string filePath = Path.Combine(_imageDirectory, fileName);

                // Check if file exists before deleting
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}