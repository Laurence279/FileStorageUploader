namespace FileStorageUploader.Core
{
    public class FileSystemService : IFileSystemService
    {
        public string[] GetFilesFromPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    return files;
                }
                else if (File.Exists(path))
                {
                    return [path];
                }
                else
                {
                    Console.WriteLine($"Path not found: {path}");
                    return [];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting files from path '{path}': {ex.Message}");
                return [];
            }
        }
    }
}
