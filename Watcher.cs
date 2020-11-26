using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;

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

    public class Watcher
    {
        Settings settings;

        public Watcher()
        {
            string fileName;
            string jsonString;

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

        public void Run()
        {
            Console.WriteLine(settings.sender[0].id);
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = Path.GetFullPath(settings.sender[0].filePath);
                watcher.Filter = settings.sender[0].filePattern;
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;


                Console.WriteLine("Press 'q' + 'Enter' to quit the application.");
                while (Console.Read() != 'q') ;

            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);
            Senders.OstfoldEnergi data = new Senders.OstfoldEnergi(e.FullPath);
        }
    }

}