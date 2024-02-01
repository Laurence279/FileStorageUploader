using System.IO;
using System.Threading.Tasks;
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
            var blobClient = this.GetBlobClient(container, fileName);

            await blobClient.UploadAsync(stream, true);
            return blobClient.Uri.AbsoluteUri;
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
