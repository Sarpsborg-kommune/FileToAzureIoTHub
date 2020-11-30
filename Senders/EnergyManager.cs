using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;

namespace FileToAzureIoTHub.Senders
{
    public class Measurement
    {
        public DateTime ts { get; set; }
        public Double v { get; set; }
    }

    public class EnergyManager
    {
        public Dictionary<string, List<Measurement>> data { get; set; }

        public string toJson() => JsonSerializer.Serialize(data);
    }
}