using FileStorageUploader.Core.Enums;
using FileStorageUploader.Core.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework.Constraints;

namespace FileStorageUploader.Tests.Services
{
    [TestFixture]
    public class FileSystemServiceTests
    {
        private class MockFileStorageService : IFileStorageService
        {
            public bool FileExists { get; set; }
            public Task<bool> ExistsAsync(string container, string path)
            {
                return Task.FromResult(this.FileExists);
            }

            public event Action<int>? UploadProgressChanged;

            public Task<string> UploadAsync(string container, string path, Stream file)
            {
                return Task.FromResult("dummy-url");
            }
        }

        private class MockUserInteractionService : IUserInteractionService
        {
            public bool UserConfirmed { get; set; }
            public bool Confirm(string prompt)
            {
                return this.UserConfirmed;
            }

            public string GetInput(string prompt)
            {
                return string.Empty;
            }

            public void PrintLine(string message)
            {
                return;
            }

            public void Print(string message)
            {
                return;
            }

            public void WaitForKey()
            {
                return;
            }
        }

        private IConfiguration configuration;
        private IFileStorageService fileStorageService;
        private IUserInteractionService userInteractionService;
        private IFileSystemService fileSystemService;

        [SetUp]
        public void SetUp()
        {
            configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            configuration["ContainerName"] = "container";
            fileStorageService = new MockFileStorageService();
            userInteractionService = new MockUserInteractionService();
            fileSystemService = new FileSystemService(configuration, fileStorageService, userInteractionService);
        }

        [Test]
        public void GetFilesFromPath_EmptyPath_ReturnsEmptyArray()
        {
            var expected = Array.Empty<string>();
            var result = fileSystemService.GetFilesFromPath(string.Empty);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task UploadFiles_FileExistsAndUserDeclinesOverwrite_DoesNotUploadFileAsync()
        {
            string filePath = Path.GetTempFileName();
            var fileStorageService = new MockFileStorageService();
            fileStorageService.FileExists = true;

            var userInteractionService = new MockUserInteractionService();
            userInteractionService.UserConfirmed = false;

            var fileSystemService = new FileSystemService(configuration, fileStorageService, userInteractionService);

            var result = await fileSystemService.UploadFiles([filePath], string.Empty);
            Assert.That(result, Is.Empty);
        }
    }
}