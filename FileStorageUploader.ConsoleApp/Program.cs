using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FileStorageUploader.Core;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace FileStorageUploader.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                await host.StartAsync();
                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var storageService = host.Services.GetRequiredService<IFileStorageService>();
                var fileSystemService = host.Services.GetRequiredService<IFileSystemService>();
                var container = host.Services.GetRequiredService<IConfiguration>().GetValue("ContainerName", "FileUploaderFiles") ?? "FileUploaderFiles";
                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                await Start(storageService, fileSystemService, logger, container);

                lifetime.StopApplication();
                await host.WaitForShutdownAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IFileStorageService, AzureFileStorageService>();
                    services.AddSingleton<IFileSystemService, FileSystemService>();
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddUserSecrets<Program>();
                })
                .UseConsoleLifetime();
        }

        private static async Task Start(
            IFileStorageService storageService, 
            IFileSystemService fileSystemService, 
            ILogger logger, 
            string container)
        {
            var files = fileSystemService.GetFilesFromPath(GetInput("Enter path to File"));
            if (files.Length <= 0)
            {
                await Start(storageService, fileSystemService, logger, container);
                return;
            }

            foreach (var fsPath in files)
            {
                var file = File.OpenRead(fsPath);
                var fileName = Path.GetFileName(file.Name);

                if (!ConfirmFileName(fileName))
                {
                    await Start(storageService, fileSystemService, logger, container);
                    return;
                }

                var dir = GetInput("[Optional]: Enter file storage directory");
                var filePath = Path.Combine(dir, fileName);

                var exists = await storageService.ExistsAsync(container, filePath);
                if (exists && !ConfirmFileOverwrite())
                {
                    await Start(storageService, fileSystemService, logger, container);
                    return;
                }

                Console.WriteLine("Uploading file..");
                var url = await storageService.UploadAsync(container, filePath, file);
                Console.WriteLine($"Uploaded file to {url}{Environment.NewLine}Press any key to close..");
                Console.ReadKey();
            }
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

        private static string GetInput(string prompt)
        {
            Console.WriteLine("{0}", prompt);
            var result = Console.ReadLine();
            return result ?? string.Empty;
        }
    }
}
