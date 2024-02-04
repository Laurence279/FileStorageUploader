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
            var host = CreateHostBuilder(args).Build();
            
            var fileSystemService = host.Services.GetRequiredService<IFileSystemService>();
            await fileSystemService.Run();
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

        private static bool Confirm(string prompt)
        {
            do
            {
                Console.WriteLine($"{0} (Y/N)", prompt);
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
