using Microsoft.Extensions.Configuration;
using static FileStorageUploader.Core.UserInteraction;

namespace FileStorageUploader.Core
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IFileStorageService storageService;
        private readonly string container;

        public FileSystemService(IConfiguration config, IFileStorageService storageService)
        {
            this.storageService = storageService;
            this.container = config["ContainerName"] ?? "";
            if (container == string.Empty)
            {
                throw new ArgumentException("Please specify a container name");
            }
        }

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
                    Print($"Path not found: {path}");
                    return [];
                }
            }
            catch (Exception ex)
            {
                Print($"Error getting files from path '{path}': {ex.Message}");
                return [];
            }
        }

        public async Task Run()
        {
            var files = this.GetFilesFromPath(GetInput("Enter path to file or directory"));
            if (files.Length <= 0)
            {
                await Run();
                return;
            }

            foreach (var fsPath in files)
            {
                var file = File.OpenRead(fsPath);
                var fileName = Path.GetFileName(file.Name);

                if (!Confirm($"Found {fileName}{Environment.NewLine}Do you want to continue?"))
                {
                    await Run();
                    return;
                }

                var dir = GetInput("[Optional]: Enter file storage path to upload to, leave blank for root directory.");
                var filePath = Path.Combine(dir, fileName);

                var exists = await storageService.ExistsAsync(container, filePath);
                if (exists && !Confirm("File already exists. Overwrite?"))
                {
                    await Run();
                    return;
                }

                Print("Uploading file..");
                var url = await storageService.UploadAsync(container, filePath, file);
                Print($"Uploaded file to {url}{Environment.NewLine}Press any key to close..");
                WaitForKey();
            }
        }
    }
}
