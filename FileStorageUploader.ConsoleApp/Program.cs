using FileStorageUploader.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    services.AddSingleton<IUserInteractionService, UserInteractionService>();
                })
                .ConfigureHostConfiguration(configBuilder =>
                {
                    configBuilder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddIniFile("config.ini", optional: false, reloadOnChange: true);
                })
                .UseConsoleLifetime();
        }
    }
}
