using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;


namespace FileToAzureIoTHub.Senders
{
    public class Maledata
    {
        public int id { get; set; }
        [JsonPropertyName("k-nummer")]
        public string knummer { get; set; }
        public string dato { get; set; }
        public string periode { get; set; }
        public int forbruk { get; set; }

        [JsonPropertyName("målerstad")]
        public int malerstand { get; set; }
    }

    public class SenderData
    {
        [JsonPropertyName("måledata")]
        public IList<Maledata> maledata { get; set; }
    }

    public class OstfoldEnergi
    {
        SenderData data;
        string jsonString;

        public OstfoldEnergi(string fileName)
        {
            if (File.Exists(fileName))
            {
                jsonString = File.ReadAllText(fileName, Encoding.Latin1);
                data = JsonSerializer.Deserialize<SenderData>(jsonString);
                //Console.WriteLine(JsonSerializer.Serialize(data));
            }
            else
                throw new System.IO.IOException($"The file {fileName} does not exist.");
        }
    }

}