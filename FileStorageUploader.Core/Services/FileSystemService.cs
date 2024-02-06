using FileStorageUploader.Core.Enums;
using Microsoft.Extensions.Configuration;
using static FileStorageUploader.Core.Helpers.UserInteraction;

namespace FileStorageUploader.Core.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IFileStorageService storageService;
        private readonly string container;


        public FileSystemService(IConfiguration config, IFileStorageService storageService)
        {
            this.storageService = storageService;
            container = config["ContainerName"] ?? "";
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
                PrintLine($"Error getting files from path '{path}': {ex.Message}");
                return [];
            }
        }

        public async Task Run()
        {
            var files = GetFilesFromPath(GetInput("Enter path to file or directory"));
            switch (files.Length)
            {
                case 0:
                    {
                        PrintLine("No files found.");
                        await Run();
                        return;
                    }
                case 1:
                    {
                        PrintLine($"Found 1 file.");
                        break;
                    }
                case > 1:
                    {
                        PrintLine($"Found {files.Length} files.");
                        break;
                    }
            }

            if (!Confirm($"Do you want to continue?"))
            {
                await Run();
                return;
            }

            var dir = GetInput("[Optional]: Enter file storage path to upload to, leave blank for root.");

            var overwriteOption = OverwriteOption.Undefined;

            for (var i = 0; i < files.Length; i++)
            {
                var file = File.OpenRead(files[i]);
                var fileName = Path.GetFileName(file.Name);
                var filePath = Path.Combine(dir, fileName);

                var exists = await storageService.ExistsAsync(container, filePath);

                if (exists && overwriteOption == OverwriteOption.Undefined)
                {
                    overwriteOption = Confirm("One or more files already exist. Overwrite existing files?") ? OverwriteOption.Overwrite : OverwriteOption.Skip;
                }
                if (exists && overwriteOption == OverwriteOption.Skip)
                {
                    PrintLine($"Skipping file {i + 1} of {files.Length}");
                    continue;
                }

                storageService.UploadProgressChanged += (percentage) => HandleProgressUpdated(percentage, i + 1, files.Length);
                var url = await storageService.UploadAsync(container, filePath, file);
            }
            PrintLine($"{Environment.NewLine}Finished processing all files. {Environment.NewLine}Press any key to close..");
            WaitForKey();
        }

        private static void HandleProgressUpdated(int percentage, int fileNumber, int filesRemaining)
        {
            Print($"\rUploading file {fileNumber}/{filesRemaining}: {percentage}% ");
        }
    }
}
