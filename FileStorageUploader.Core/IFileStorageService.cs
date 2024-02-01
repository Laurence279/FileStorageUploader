namespace FileStorageUploader.Core
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(string container, string fileName, Stream stream);

        Task<bool> ExistsAsync(string container, string fileName);
    }
}
