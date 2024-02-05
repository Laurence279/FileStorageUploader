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
    }
}
