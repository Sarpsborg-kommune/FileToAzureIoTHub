# FileToAzureIoTHub

Rewrite of filemsgtoazureiothub. The new FileToAzureIoTHub moves implementation from Python to C#
and .NET5. This will deprecate filemsgtoazureiothub when finnished. The project is not _generic_
and will only work with filetypes from two vendors: Østfold Energi and Storm Elektro. Thus public interest should only be as an example of how to move data from a file to Azure IoT Hub when the
file is created.

To Install .Net on Linux, please follow Microsofts instructions (google it).

# ConfigurationFile

The configuration file `FileToAzureIoTHub.json` must be placed differently based on the OS the
file is installed on:<br/>
Linux: `/etc/FileToAzureIoTHub.json`<br/>
Windows: `$env:ProgramData/Sarpsborgkommune/FileToAzureIoTHub/FileToAzureIoTHub.json`<br/>

The file format is:

```json
{
    "sender": [
        {
            "id": "",
            "filePath": "",
            "filePattern": ""
        },
        {
            ...
        }
    ],
    "receiver": [
        {
            "id": "",
            "connectionString": ""
        }
    ]
}
```

Currently only two senders are implemented:<br\>
ostfoldeneergi, smartelektro<br\>

Currently only one receiver is implemented:<br\>
energymanager<br\>
