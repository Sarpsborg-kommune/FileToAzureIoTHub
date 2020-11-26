using System;
using System.IO;

namespace FileToAzureIoTHub
{

    public class Program
    {
        static void Main()
        {
            Watcher watcher = new Watcher();

            watcher.Run();
        }

    }
}