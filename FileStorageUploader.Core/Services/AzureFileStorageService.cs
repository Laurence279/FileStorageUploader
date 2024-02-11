using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace FileStorageUploader.Core.Services
{
    public class AzureFileStorageService : IFileStorageService
    {
        private readonly string connectionString;
        private readonly string storageContainerName;
        public event Action<int>? UploadProgressChanged;

        public AzureFileStorageService(IConfiguration config)
        {
            connectionString = config["ConnectionString"] ?? "";
            storageContainerName = config["ContainerName"] ?? "";

            InitContainer().Wait();
        }

        private async Task<bool> InitContainer()
        {
            var blobClient = new BlobContainerClient(connectionString, storageContainerName);
            await blobClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            return true;
        }

        public async Task<string> UploadAsync(string container, string fileName, Stream stream)
        {
            var progress = new Progress<long>();
            var percentageRef = 0;
            progress.ProgressChanged += (sender, e) => HandleProgressChanged(sender, e, stream.Length, ref percentageRef);

            var blobClient = GetBlobClient(container, fileName);

            await blobClient.UploadAsync(stream, progressHandler: progress);
            return blobClient.Uri.AbsoluteUri;
        }

        private void HandleProgressChanged(object? sender, double e, double size, ref int prevVal)
        {
            var percentage = (int)Math.Round(e / size * 100);
            if (percentage == prevVal) return;
            prevVal = percentage;
            UploadProgressChanged?.Invoke(percentage);
        }

        public async Task<bool> ExistsAsync(string container, string fileName)
        {
            return await GetBlobClient(container, fileName).ExistsAsync();
        }

        private BlobClient GetBlobClient(string container, string filename)
        {
            var containerClient = new BlobContainerClient(connectionString, container);
            return containerClient.GetBlobClient(filename);
        }
    }
}
