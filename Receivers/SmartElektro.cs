using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FileToAzureIoTHub.Receivers
{
    public class SeData
    {
        public DateTime Timestamp { get; set; }
        public string Id { get; set; }
        public double Value { get; set; }
    }

    public class SmartElektro
    {
        public List<SeData> sedata { get; set; }

        public SmartElektro(string fileName)
        {
            sedata = new List<SeData>();

            foreach (string line in File.ReadLines(fileName))
            {
                string[] item = line.Split(',');

                string matchString1 = "_Total_Active_Energy_Import_T";
                string matchString2 = "Cluster1.";
                if (item[1].Contains(matchString1))
                {
                    SeData record = new SeData();
                    record.Timestamp = DateTime.Parse(item[0]).ToUniversalTime();
                    record.Id = item[1].Replace(matchString1, String.Empty).Replace(matchString2, String.Empty);
                    record.Value = Convert.ToDouble(item[2], CultureInfo.InvariantCulture);
                    sedata.Add(record);
                }
            }
        }
    }


}