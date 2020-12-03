# FileToAzureIoTHub

Rewrite of filemsgtoazureiothub. The new FileToAzureIoTHub moves implementation from Python to C#
and .NET5. This will deprecate filemsgtoazureiothub when finnished. The project is not _generic_
and will only work with filetypes from two vendors: Østfold Energi and Storm Elektro. Thus public interest should only be as an example of how to move data from a file to Azure IoT Hub when the
file is created.

## Prerequisites

.NET 5 SDK and PowerShell 7 must be installed on the server. To Install .Net and Powershell on Linux or Windows, please follow Microsofts instructions (google it). You can use the latest
versions. For Ubuntu: `# sudo apt get dotnet-sdk-5.0`

Git must be installed to download the needed files.

## Installation

Clone the repository: `git clone ...`<br/>
`cd FileToAzureIoTHub`<br/>

**Linux Specific**
Build the project: `dotnet build --configuration Release --runtime linux-x64`<br/>
Copy binary files to /opt/FileToAzureIoTHub:<br/>
`cp -R ./bin/Release/net5.0/linux-x64 /opt/FileToAzureIoTHub`<br/>
Copy FileToAzureIoTHub.service to /etc/systemd/system/:<br/>
`sudo cp FileToAzureIoTHub.service /etc/systemd/system`<br/>
Copy the configuration file to /etc and edit it:<br/>
`sudo cp ConfigExample.json /etc/FileToAzureIoTHub.json`<br/>
Enable and start the servie<br/>
`sudo systemctl enable FileToAzureIoTHub` <br/>
`sudo systemctl start FileToAzureIoTHub`<br/>
Check if everything is running:<br/>
`sudo systemctl statue FileToAzureIoTHub`<br/>
`sudo journalctl -r -u FileToAzureIoTHub`<br/>

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
    "receiver": [
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
    "sender": [
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

Currently only two receivers are implemented:<br/>
ostfoldeneergi, smartelektro<br/>

Currently only one sender is implemented:<br/>
energymanager<br/>

## Sender

## Receiver

### Ostfold Energi

This json format does not adhere to standards. The format uses Norwegian special characters, and
this should be avoided.
The dataformat for the messages is:

```json
{
    "måledata":[
        {
            "id": int,
            "k-nummer": "<string>",
            "dato": "<string>DD.MM.YYY",
            "periode": "<string>HH:MM",
            "forbruk": int,
            "målerstand": int
        },
        ...
    ]
}
```

### EnergyManager

EnergyManager must receive messages in the following json format.

```json
{
    "<måler-id>": [
        {"ts": <datetime>, "v": <double>},
        { ... },
        ...
    ],
    "<måler-id<": [
        {"ts": <datetime>, "v": <double>},
        { ... },
        ...
    ],
    ...
}
```

Multisensordata can also be formatted as:

```json
{
    "<måler-id>": [
        {"ts": <datetime>, "v": { "<key1>": <double>, "<key2>": <double>, ...}}
    ]
}
```

### Smart Elektro

The data is received as a CSV file with the following format.

```
"<DateTime>","<IDENT>",<METER READING>
"DD-MM-YYYY HH:MM:SS","<IDENT String>",<double>
```

The Ident String has the following format:
`Cluster<#>.ID_MEASUREMENTTYPE`

ID example: `BY3570_310_001_OE001`

MEASUREMENTTYPE examples: `Heat_energy_Y`, `kwh_T`

The Code uses the CsvHelper library: https://joshclose.github.io/CsvHelper/.
