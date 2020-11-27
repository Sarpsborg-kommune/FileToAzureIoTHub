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
    public class Sender
    {
        public string id { get; set; }
        public string filePath { get; set; }
        public string filePattern { get; set; }
    }

    public class Receiver
    {
        public string id { get; set; }
        public string connectionString { get; set; }
    }

    public class Settings
    {
        public IList<Sender> sender { get; set; }
        public IList<Receiver> receiver { get; set; }
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
            }
            else
                throw new System.IO.IOException("Configuration File does not exist.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = Path.GetFullPath(settings.sender[0].filePath);
                watcher.Filter = settings.sender[0].filePattern;
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;

                _logger.LogInformation("{time} FileToAzureIoTHub service started.", DateTimeOffset.Now);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
                _logger.LogInformation("{time} FileToAzureIoTHub service stopping.", DateTimeOffset.Now);
            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);
            Senders.OstfoldEnergi data = new Senders.OstfoldEnergi(e.FullPath);
        }
    }

}

/*
    Console.WriteLine("Press 'q' + 'Enter' to quit the application.");
    while (Console.Read() != 'q') ;

*/