using FileStorageUploader.Core.Services;
using Microsoft.Extensions.Configuration;

namespace FileStorageUploader.Tests.Services
{
    [TestFixture]
    public class FileSystemServiceTests
    {
        private class MockFileStorageService : IFileStorageService
        {
            public bool FileExistsResult { get; set; }
            public Task<bool> ExistsAsync(string container, string path)
            {
                return Task.FromResult(this.FileExistsResult);
            }

            public event Action<int>? UploadProgressChanged;

            public Task<string> UploadAsync(string container, string path, Stream file)
            {
                return Task.FromResult("dummy-url");
            }
        }

        private class MockUserInteractionService : IUserInteractionService
        {
            public bool UserConfirmResult { get; set; }
            public bool Confirm(string prompt)
            {
                return this.UserConfirmResult;
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
        private MockFileStorageService fileStorageService;
        private MockUserInteractionService userInteractionService;
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
            var result = fileSystemService.GetFilesFromPath(string.Empty);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task UploadFiles_FileExistsAndUserDeclinesOverwrite_DoesNotProcessFile()
        {
            string filePath = Path.GetTempFileName();
            this.fileStorageService.FileExistsResult = true;
            this.userInteractionService.UserConfirmResult = false;

            var result = await this.fileSystemService.ProcessFiles([filePath], string.Empty);
            Assert.That(result, Is.Empty);
        }
    }
}