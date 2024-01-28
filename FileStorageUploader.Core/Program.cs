using FileStorageUploader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Pipes;

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

            var file = GetFileFromPath(GetInput("Enter path to File"));
            if (file == null)
            {
                await Init();
                return;
            }

            var fileName = Path.GetFileName(file.Name);
            if (!ConfirmFileName(fileName))
            {
                await Init();
                return;
            }

            var dir = GetInput("[Optional]: Enter file storage directory");
            var filePath = Path.Combine(dir, fileName);

            var exists = await storageService.ExistsAsync(container, filePath);
            if (exists && !ConfirmFileOverwrite())
            {
                await Init();
                return;
            }

            Console.WriteLine("Uploading file..");
            var url = await storageService.UploadAsync(container, filePath, file);
            Console.WriteLine($"Uploaded file to {url}{Environment.NewLine}Press any key to close..");
            Console.ReadKey();
        }

        private static bool ConfirmFileOverwrite()
        {
            do
            {
                Console.WriteLine("File already exists. Overwrite? (Y/N)");
                var key = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (key == 'N')
                {
                    return false;
                }
                else if (key == 'Y')
                {
                    break;
                }
            } while (true);
            return true;
        }

        private static bool ConfirmFileName(string fileName)
        {
            do
            {
                Console.WriteLine($"Found {fileName}{Environment.NewLine}Do you want to continue? (Y/N)");
                var key = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (key == 'Y')
                {
                    break;
                }
                else if (key == 'N')
                {
                    return false;
                }
            } while (true);
            return true;
        }

        private static FileStream? GetFileFromPath(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist. Please try again.");
                return null;
            }
            return File.OpenRead(path);
        }

        private static string GetInput(string prompt)
        {
            Console.WriteLine("{0}", prompt);
            var result = Console.ReadLine();
            return result ?? string.Empty;
        }
    }
}
