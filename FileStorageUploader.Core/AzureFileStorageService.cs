using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace FileStorageUploader.Core
{
    public class AzureFileStorageService : IFileStorageService
    {
        private readonly string connectionString;
        private readonly string storageContainerName;

        public AzureFileStorageService(IConfiguration config)
        {
            this.connectionString = config["ConnectionString"] ?? "";
            this.storageContainerName = config["ContainerName"] ?? "";

            InitContainer().Wait();
        }

        private async Task<bool> InitContainer()
        {
            var blobClient = new BlobContainerClient(this.connectionString, this.storageContainerName);
            await blobClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            return true;
        }

        public async Task<string> UploadAsync(string container, string fileName, Stream stream)
        {
            var progress = new Progress<long>();
            var percentageRef = 0;
            progress.ProgressChanged += (sender, e) => HandleProgressChanged(sender, e, stream.Length, ref percentageRef);

            var blobClient = this.GetBlobClient(container, fileName);

            await blobClient.UploadAsync(stream, progressHandler: progress);
            return blobClient.Uri.AbsoluteUri;
        }

        private void HandleProgressChanged(object? sender, double e, double size, ref int prevVal)
        {
            var percentage = (int)Math.Round((e / size) * 100);
            if (percentage == prevVal) return;
            Console.Write($"\r{percentage}% ");
            prevVal = percentage;
        }

        public async Task<bool> ExistsAsync(string container, string fileName)
        {
            return await this.GetBlobClient(container, fileName).ExistsAsync();
        }

        private BlobClient GetBlobClient(string container, string filename)
        {
            var containerClient = new BlobContainerClient(this.connectionString, container);
            return containerClient.GetBlobClient(filename);
        }
    }
}
