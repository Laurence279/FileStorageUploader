using FileStorageUploader.Core.Enums;
using Microsoft.Extensions.Configuration;

namespace FileStorageUploader.Core.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly IFileStorageService storageService;
        private readonly IUserInteractionService userInteractionService;
        private readonly string container;


        public FileSystemService(IConfiguration config, IFileStorageService storageService, IUserInteractionService userInteractionService)
        {
            this.storageService = storageService;
            this.userInteractionService = userInteractionService;
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
                    this.userInteractionService.Print($"Path not found: {path}");
                    return [];
                }
            }
            catch (Exception ex)
            {
                this.userInteractionService.PrintLine($"Error getting files from path '{path}': {ex.Message}");
                return [];
            }
        }

        public async Task Run()
        {
            var files = GetFilesFromPath(this.userInteractionService.GetInput("Enter path to file or directory"));
            switch (files.Length)
            {
                case 0:
                    {
                        this.userInteractionService.PrintLine("No files found.");
                        await Run();
                        return;
                    }
                case 1:
                    {
                        this.userInteractionService.PrintLine($"Found 1 file.");
                        break;
                    }
                case > 1:
                    {
                        this.userInteractionService.PrintLine($"Found {files.Length} files.");
                        break;
                    }
            }

            if (!this.userInteractionService.Confirm($"Do you want to continue?"))
            {
                await Run();
                return;
            }

            var dir = this.userInteractionService.GetInput("[Optional]: Enter file storage path to upload to, leave blank for root.");
            await ProcessFiles(files, dir);
            this.userInteractionService.PrintLine($"{Environment.NewLine}Finished processing all files. {Environment.NewLine}Press any key to close..");
            this.userInteractionService.WaitForKey();
        }

        public async Task<List<string>> ProcessFiles(string[] files, string dir, OverwriteOption? overwriteOption = OverwriteOption.Undefined)
        {
            var uploadedFiles = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                var file = File.OpenRead(files[i]);
                var fileName = Path.GetFileName(file.Name);
                var filePath = Path.Combine(dir, fileName);

                var exists = await storageService.ExistsAsync(container, filePath);

                if (exists && overwriteOption == OverwriteOption.Undefined)
                {
                    overwriteOption = this.userInteractionService.Confirm("One or more files already exist. Overwrite existing files?") ? OverwriteOption.Overwrite : OverwriteOption.Skip;
                }
                if (exists && overwriteOption == OverwriteOption.Skip)
                {
                    this.userInteractionService.PrintLine($"Skipping file {i + 1} of {files.Length}");
                    continue;
                }

                storageService.UploadProgressChanged += (percentage) => HandleProgressUpdated(percentage, i + 1, files.Length);
                await storageService.UploadAsync(container, filePath, file);
                uploadedFiles.Add(files[i]);
            }
            return uploadedFiles;
        }

        private void HandleProgressUpdated(int percentage, int fileNumber, int filesRemaining)
        {
            this.userInteractionService.Print($"\rUploading file {fileNumber}/{filesRemaining}: {percentage}% ");
        }
    }
}
