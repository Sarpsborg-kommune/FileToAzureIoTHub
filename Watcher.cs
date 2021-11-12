using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
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



        public Watcher(ILogger<Watcher> logger, IHostLifetime lifetime)
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
                _logger.LogInformation("{time} FileToAzureIoTHub Version 1.1", DateTimeOffset.Now);
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
            oewatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            oewatcher.Created += OnCreated_OstfoldEnergi;
            oewatcher.EnableRaisingEvents = true;

            sswatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            sswatcher.Path = Path.GetFullPath(settings.receiver[1].filePath);
            sswatcher.Filter = settings.receiver[1].filePattern;
            sswatcher.Created += OnCreated_SmartElektro;
            sswatcher.EnableRaisingEvents = true;

            _logger.LogInformation("{time} FileToAzureIoTHub service started.", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            _logger.LogInformation("Stopping FileToAzureIoTHub service.");
        }

        private void OnCreated_OstfoldEnergi(object source, FileSystemEventArgs e)
        {


            Thread.Sleep(3000);         // Wait to be sure the file is closed.
            if (File.Exists(e.FullPath))
            {
                if (new FileInfo(e.FullPath).Length == 0)
                {
                    _logger.LogWarning($"File: {e.FullPath} har 0 size. Abandoning file.");
                }
                else
                {
                    _logger.LogInformation($"File: {e.FullPath} received.");
                    try
                    {
                        Receivers.OstfoldEnergi r_data = new Receivers.OstfoldEnergi(e.FullPath);
                        Senders.EnergyManager s_data = new Senders.EnergyManager();
                        s_data.data = new Dictionary<string, List<Senders.Measurement>>();
                        string dataids = "";

                        foreach (Receivers.Maledata item in r_data.data.maledata)
                        {
                            if (item.malerstand > 0)
                            {
                                Senders.Measurement s_item = new Senders.Measurement();
                                List<Senders.Measurement> s_item_list = new List<Senders.Measurement>();

                                s_item.ts = DateTime.Parse($"{item.dato} {item.periode}").ToUniversalTime();
                                s_item.v = item.malerstand;

                                s_item_list.Add(s_item);
                                s_data.data.Add(item.id.ToString(), s_item_list);
                                dataids += $"[{item.id.ToString()}] ";
                            }
                        }
                        _logger.LogInformation($"Received ID(s): {dataids}");
                        SendDataToIotHub(s_data.toJson());
                        MoveFileToOld(e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unexpected exception when processing file: {e.FullPath}");
                        _logger.LogError($"Exception is: {ex}");
                        _logger.LogError($"Message: {ex.Message}");
                        _logger.LogError($"Stacktrace: {ex.StackTrace}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"File: {e.FullPath} does not exist.");
            }

        }

        private void OnCreated_SmartElektro(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);     // Wait to be sure the file is closed.
            if (File.Exists(e.FullPath))
            {
                if (new FileInfo(e.FullPath).Length == 0)
                {
                    _logger.LogWarning($"File: {e.FullPath} har 0 size. Abandoning file.");
                }
                else
                {
                    _logger.LogInformation($"File: {e.FullPath} received.");
                    try
                    {
                        Receivers.SmartElektro r_data = new Receivers.SmartElektro(e.FullPath);
                        Senders.EnergyManager s_data = new Senders.EnergyManager();
                        s_data.data = new Dictionary<string, List<Senders.Measurement>>();
                        string dataids = "";

                        foreach (Receivers.SeData item in r_data.sedata)
                        {
                            Senders.Measurement s_item = new Senders.Measurement();
                            List<Senders.Measurement> s_item_list = new List<Senders.Measurement>();

                            s_item.ts = item.Timestamp;
                            s_item.v = item.Value;

                            if (s_data.data.ContainsKey(item.Id))
                            {
                                s_data.data[item.Id].Add(s_item);
                                dataids += $"[{item.Id}] ";
                            }
                            else
                            {
                                s_item_list.Add(s_item);
                                s_data.data.Add(item.Id, s_item_list);
                            }
                        }
                        _logger.LogInformation($"Received ID(s): {dataids}");
                        SendDataToIotHub(s_data.toJson());
                        MoveFileToOld(e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unexpected exception when processing file: {e.FullPath}");
                        _logger.LogError($"Exception is: {ex}");
                        _logger.LogError($"Message: {ex.Message}");
                        _logger.LogError($"Stacktrace: {ex.StackTrace}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"File: {e.FullPath} does not exist.");
            }

        }

        private async void SendDataToIotHub(string jsonData)
        {
            DeviceClient iotClient;

            try
            {
                _logger.LogInformation("Sending data to AzurIoTHub.");
                iotClient = DeviceClient.CreateFromConnectionString(Program.connectionString);
                var message = new Message(Encoding.ASCII.GetBytes(jsonData));
                await iotClient.SendEventAsync(message);
                _logger.LogInformation("Data sendt to Azure IoTHub.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to send data to Azure IoT Hub.");
                _logger.LogError($"Exception is: {ex}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"Stacktrace: {ex.StackTrace}");
            }
        }

        private static bool IsFileLocked(string fileName)
        {
            try
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fileStream != null) fileStream.Close();
                }
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
            }
            return true;
        }

        private void MoveFileToOld(string fileName)
        {
            string oldpath;

            try
            {
                oldpath = Path.GetDirectoryName(fileName);
                oldpath = oldpath + Path.DirectorySeparatorChar + "old" + Path.DirectorySeparatorChar + Path.GetFileName(fileName);
                _logger.LogInformation($"Moving {fileName} to {oldpath}");
                File.Move(fileName, oldpath, true);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception {ex}when trying to file.\n{ex.Message}\n{ex.StackTrace}");
            }
        }

    }

}