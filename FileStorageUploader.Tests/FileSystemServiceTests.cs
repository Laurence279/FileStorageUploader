using FileStorageUploader.Core.Services;
using Microsoft.Extensions.Configuration;

namespace FileStorageUploader.Tests.Services
{
    [TestFixture]
    public class FileSystemServiceTests
    {
        private class MockFileStorageService : IFileStorageService
        {
            public Task<bool> ExistsAsync(string container, string path)
            {
                return Task.FromResult(false);
            }

            public event Action<int>? UploadProgressChanged;

            public Task<string> UploadAsync(string container, string path, Stream file)
            {
                return Task.FromResult("dummy-url");
            }
        }

        private IConfiguration configuration;
        private IFileStorageService fileStorageService;
        private FileSystemService fileSystemService;

        [SetUp]
        public void SetUp()
        {
            configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            configuration["ContainerName"] = "container";
            fileStorageService = new MockFileStorageService();
            fileSystemService = new FileSystemService(configuration, fileStorageService);
        }

        [Test]
        public void GetFilesFromPath_EmptyPath_ReturnsEmptyArray()
        {
            var expected = Array.Empty<string>();
            var result = fileSystemService.GetFilesFromPath("");

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}