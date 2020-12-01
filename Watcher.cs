using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices.Client;

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

            Program.connectionString = settings.sender[0].connectionString;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using FileSystemWatcher oewatcher = new FileSystemWatcher(),
                                    sswatcher = new FileSystemWatcher();

            oewatcher.Path = Path.GetFullPath(settings.receiver[0].filePath);
            oewatcher.Filter = settings.receiver[0].filePattern;
            oewatcher.Created += OnCreated_OstfoldEnergi;
            oewatcher.EnableRaisingEvents = true;

            sswatcher.Path = Path.GetFullPath(settings.receiver[1].filePath);
            sswatcher.Filter = settings.receiver[1].filePattern;
            sswatcher.Created += OnCreated_SmartElektro;
            sswatcher.EnableRaisingEvents = true;

            _logger.LogInformation("{time} FileToAzureIoTHub service started.", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            _logger.LogInformation("{time} FileToAzureIoTHub service stopping.", DateTimeOffset.Now);
        }

        private static void OnCreated_OstfoldEnergi(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);         // Wait to be sure the file is closed.
            Receivers.OstfoldEnergi r_data = new Receivers.OstfoldEnergi(e.FullPath);
            Senders.EnergyManager s_data = new Senders.EnergyManager();
            s_data.data = new Dictionary<string, List<Senders.Measurement>>();

            foreach (Receivers.Maledata item in r_data.data.maledata)
            {

                Senders.Measurement s_item = new Senders.Measurement();
                List<Senders.Measurement> s_item_list = new List<Senders.Measurement>();

                s_item.ts = DateTime.Parse($"{item.dato} {item.periode}").ToUniversalTime();
                s_item.v = item.malerstand;

                s_item_list.Add(s_item);
                s_data.data.Add(item.id.ToString(), s_item_list);
            }
            SendDataToIotHub(s_data.toJson());
        }

        private static void OnCreated_SmartElektro(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);     // Wait to be sure the file is closed.
            Receivers.SmartElektro r_data = new Receivers.SmartElektro(e.FullPath);
            Senders.EnergyManager s_data = new Senders.EnergyManager();
            s_data.data = new Dictionary<string, List<Senders.Measurement>>();


            foreach (Receivers.SeData item in r_data.sedata)
            {
                Senders.Measurement s_item = new Senders.Measurement();
                List<Senders.Measurement> s_item_list = new List<Senders.Measurement>();
                s_item.ts = item.Timestamp;
                s_item.v = item.Value;

                if (s_data.data.ContainsKey(item.Id))
                {
                    s_data.data[item.Id].Add(s_item);
                }
                else
                {
                    s_item_list.Add(s_item);
                    s_data.data.Add(item.Id, s_item_list);
                }
            }
            SendDataToIotHub(s_data.toJson());
        }

        private static async void SendDataToIotHub(string jsonData)
        {
            DeviceClient iotClient;

            try
            {
                iotClient = DeviceClient.CreateFromConnectionString(Program.connectionString);
                var message = new Message(Encoding.ASCII.GetBytes(jsonData));
                await iotClient.SendEventAsync(message);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

}