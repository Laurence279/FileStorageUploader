using Azure.Storage.Blobs.Models;

namespace FileStorageUploader.Core
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(string container, string fileName, Stream stream);

        public event Action<int>? UploadProgressChanged;

        Task<bool> ExistsAsync(string container, string fileName);
    }
}
