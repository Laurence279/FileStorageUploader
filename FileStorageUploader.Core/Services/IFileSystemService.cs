using FileStorageUploader.Core.Enums;

namespace FileStorageUploader.Core.Services
{
    public interface IFileSystemService
    {
        string[] GetFilesFromPath(string path);

        Task Run();

        Task<List<string>> ProcessFiles(string[] files, string dir, OverwriteOption? overwriteOption = OverwriteOption.Undefined);
    }
}
