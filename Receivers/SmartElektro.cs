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

                string matchString1 = "Cluster1.";
                string matchString2 = "_Total_Active_Energy_Import_T";
                string matchString3 = "_kwh_T";
                string matchString4 = "_ENERGY_T";
                string matchString5 = "_Accumulated_heat_energy_T";
                string matchString6 = "_Heat_energy_T";

                SeData record = new SeData();
                record.Timestamp = DateTime.Parse(item[0]).ToUniversalTime();
                record.Id = item[1].Replace(matchString1, String.Empty);

                if (record.Id.Contains(matchString2))
                    record.Id.Replace(matchString2, String.Empty);
                else if (record.Id.Contains(matchString3))
                    record.Id.Replace(matchString3, String.Empty);
                else if (record.Id.Contains(matchString4))
                    record.Id.Replace(matchString4, String.Empty);
                else if (record.Id.Contains(matchString5))
                    record.Id.Replace(matchString5, String.Empty);
                else if (record.Id.Contains(matchString6))
                    record.Id.Replace(matchString6, String.Empty);


                record.Value = Convert.ToDouble(item[2], CultureInfo.InvariantCulture);
                sedata.Add(record);

            }
        }
    }
}