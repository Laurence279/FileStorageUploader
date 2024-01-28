using FileStorageUploader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorageUploader.Core
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Init();
        }

        private static async Task Init()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<Program>()
            .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IFileStorageService, AzureFileStorageService>();
            var serviceProvider = services.BuildServiceProvider();

            var storageService = serviceProvider.GetRequiredService<IFileStorageService>();
            var container = configuration["ContainerName"];

            var path = GetInput("Path to File");
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist. Please try again.");
                await Init();
                return;
            }
            var fileStream = File.OpenRead(path);
            var validResponse = false;
            do
            {
                var fileName = Path.GetFileName(fileStream.Name);
                Console.WriteLine($"Found {fileName}{Environment.NewLine}Do you want to continue? (Y/N)");
                var key = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (key == 'Y')
                {
                    validResponse = true;
                    var exists = await storageService.ExistsAsync(container, fileName);
                    if (exists)
                    {
                        var validOverwritePromptResponse = false;
                        do
                        {
                            Console.WriteLine("File already exists. Overwrite? (Y/N)");
                            key = char.ToUpper(Console.ReadKey(true).KeyChar);
                            if (key == 'N')
                            {
                                await Init();
                                return;
                            }
                            else if (key == 'Y')
                            {
                                validOverwritePromptResponse = true;
                            }
                        } while (!validOverwritePromptResponse);
                    }
                    Console.WriteLine("Uploading file..");
                    var filePath = await storageService.UploadAsync(container, fileName, fileStream);
                    Console.WriteLine($"Uploaded file to {filePath}{Environment.NewLine}Press any key to close..");
                    Console.ReadKey();
                }
                else if (key == 'N')
                {
                    await Init();
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
