namespace FileStorageUploader.Core
{
    public interface IFileSystemService
    {
        string[] GetFilesFromPath(string path);

        Task Run();
    }
}
