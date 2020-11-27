using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileToAzureIoTHub
{
    public class Receiver
    {
        public string id { get; set; }
        public string filePath { get; set; }
        public string filePattern { get; set; }
    }

    public class Sender
    {
        public string id { get; set; }
        public string connectionString { get; set; }
    }

    public class Settings
    {
        public IList<Receiver> receiver { get; set; }
        public IList<Sender> sender { get; set; }

    }

    public class Watcher : BackgroundService
    {
        private readonly ILogger<Watcher> _logger;
        Settings settings;

        public Watcher(ILogger<Watcher> logger)
        {
            string fileName;
            string jsonString;

            _logger = logger;

            if (OperatingSystem.IsLinux())
            {
                fileName = "/etc/FileToAzureIoTHub.json";
            }
            else if (OperatingSystem.IsWindows())
            {
                fileName = $"{Environment.GetEnvironmentVariable("ProgramData")}\\Sarpsborgkommune\\FileToAzureIoTHub\\FileToAzureIoTHub.json";
            }
            else
            {
                throw new System.InvalidOperationException("Operating System not supported.");
            }

            if (File.Exists(fileName))
            {
                jsonString = File.ReadAllText(fileName);
                settings = JsonSerializer.Deserialize<Settings>(jsonString);
                _logger.LogInformation("{time} FileToAzureIoTHub Configuration Settings Initialized.", DateTimeOffset.Now);
                _logger.LogInformation("{time} {config}", DateTimeOffset.Now, JsonSerializer.Serialize<Settings>(settings));

            }
            else
                throw new System.IO.IOException("Configuration File does not exist.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using FileSystemWatcher oewatcher = new FileSystemWatcher(),
                                    sswatcher = new FileSystemWatcher();

            oewatcher.Path = Path.GetFullPath(settings.receiver[0].filePath);
            oewatcher.Filter = settings.receiver[0].filePattern;
            oewatcher.Created += OnCreated_OstfoldEnergi;
            oewatcher.EnableRaisingEvents = true;

            _logger.LogInformation("{time} FileToAzureIoTHub service started.", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            _logger.LogInformation("{time} FileToAzureIoTHub service stopping.", DateTimeOffset.Now);
        }

        private static void OnCreated_OstfoldEnergi(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);
            Senders.OstfoldEnergi data = new Senders.OstfoldEnergi(e.FullPath);
        }
    }

}