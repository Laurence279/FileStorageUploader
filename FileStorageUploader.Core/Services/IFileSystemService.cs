namespace FileStorageUploader.Core.Services
{
    public interface IFileSystemService
    {
        string[] GetFilesFromPath(string path);

        Task Run();
    }
}
