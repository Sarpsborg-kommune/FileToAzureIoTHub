using System;
using System.IO;

namespace FileToAzureIoTHub
{
    public class Watcher
    {
        static void Main()
        {
            Run();
        }

        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = args[1];
                watcher.Filter = "*.json";
                watcher.Created += OnCreated;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press 'q' to quit the application.");
                while (Console.Read() != 'q') ;
            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        }
    }


}
