using FileStorageUploader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorageUploader.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets <Program>()
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IFileStorageService, AzureFileStorageService>();
            var serviceProvider = services.BuildServiceProvider();

            var storageService = serviceProvider.GetRequiredService<IFileStorageService>();
            await Init(storageService);
        }

        private static async Task Init(IFileStorageService storageService)
        {
            var path = GetInput("Path to File");
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist. Please try again.");
                await Init(storageService);
            }
            var fileStream = File.OpenRead(path);
            var validResponse = false;
            do
            {
                Console.WriteLine("Found {0}", Path.GetFileName(fileStream.Name));
                Console.WriteLine("Do you want to continue? (Y/N)");
                var key = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (key == 'Y')
                {
                    validResponse = true;
                    Console.WriteLine("Uploading file..");
                    var filePath = await storageService.UploadAsync("mycontainer", "file", fileStream);
                    Console.WriteLine("Uploaded file to {0}", filePath);
                    Console.WriteLine("Press any key to close..");
                    Console.ReadKey();
                }
                else if (key == 'N')
                {
                    await Init(storageService);
                    return;
                }
            } while (!validResponse);
        }

        private static string GetInput(string prompt)
        {
            Console.WriteLine("Enter {0}", prompt);
            var result = Console.ReadLine();
            return result ?? string.Empty;
        }
    }
}
