# FileToAzureIoTHub

Rewrite of filemsgtoazureiothub. The new FileToAzureIoTHub moves implementation from Python to C#
and .NET5. This will deprecate filemsgtoazureiothub when finnished. The project is not _generic_
and will only work with filetypes from two vendors: Østfold Energi and Storm Elektro. Thus public interest should only be as an example of how to move data from a file to Azure IoT Hub when the
file is created.

## Prerequisites

.NET 5 SDK and PowerShell 7 must be installed on the server. To Install .Net and Powershell on Linux or Windows, please follow Microsofts instructions (google it). You can use the latest
versions.

Git must be installed to download the needed files.

## Installation

Clone the repository: `git clone ...`<br/>
`cd FileToAzureIoTHub`<br/>
Build the project: `dotnet build --configuration Release`<br/>
As administrator/root: `pwsh ./Install.ps1`

## Configuration File

> :warning: **Be _VERY_ careful with the config file exposure after you add theAzure IoT Hub connection string.
> This string should not be exposed to unauthorized parties. You can prevent this by only editing the file in the
> configuration directory.**

The configuration file `FileToAzureIoTHub.json` is placed differently based on the OS the
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
        },
        ...
    ],
    "receiver": [
        {
            "id": "",
            "connectionString": ""
        },
        {
            ...
        },
        ...
    ]
}
```

Currently only two senders are implemented:<br/>
ostfoldeneergi, smartelektro<br/>

Currently only one receiver is implemented:<br/>
energymanager<br/>
