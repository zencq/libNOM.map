# libNOM.map

![Maintained](https://img.shields.io/maintenance/yes/2022)
[![.NET Framework 4.7 | Standard 2.0 | 5.0 | 6.0](https://img.shields.io/badge/.NET-Framework%204.7%20%7C%20Standard%202.0%20%7C%205.0%20%7C%206.0-lightgrey)](https://dotnet.microsoft.com/en-us/)
[![C# 10.0](https://img.shields.io/badge/C%23-10.0-lightgrey)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Release](https://img.shields.io/github/v/release/zencq/libNOM.map?display_name=tag)](https://github.com/zencq/libNOM.map/releases/latest)

[![libNOM.map](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml/badge.svg)](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml)

## Introduction

The `libNOM` label is a collection of .NET class libraries originally developed
and used in [NomNom](https://github.com/zencq/NomNom), a savegame editor for [No Man's Sky](https://www.nomanssky.com/).

`libNOM.map` can be used to obfuscate and deobfuscate save file content.

## Getting Started

The mapping is stored in a singleton instance and each function is just a simple
call.

Not only the latest mapping is supported but also legacy keys that are gone in a
game version after **Beyond 2.11**. It is also possible to download an updated mapping
from the [lastest MBINCompiler release](https://github.com/monkeyman192/MBINCompiler/releases/latest).
It will be downloaded to **download/mapping.json** (if no other path is set) and
automatically loaded if present.

The built-in mapping is based on `3.88.0.2`.

### Usage

The obfuscation and deobfuscation is done in-place. Deobfuscation will return a
set of unknown keys.
```csharp
// Deobfuscate
HashSet<string> unknownKeys = Mapping.Deobfuscate(jsonObject);

// Obfuscate
Mapping.Obfuscate(jsonObject);
```

Create and update settings.
```csharp
// Settings
Mapping.Settings = new() { Download = "download" };
```

Update and download the mapping.json if a newer version is available.
```csharp
// Update
Mapping.Update();
Mapping.UpdateAsync();
```

## License

This project is licensed under the GNU GPLv3 license - see the [LICENSE](LICENSE)
file for details.

## Authors

* **Christian Engelhardt** (zencq) - [GitHub](https://github.com/cengelha)

## Credits

Thanks to the following people for their help in one way or another.

* [monkeyman192](https://github.com/monkeyman192/MBINCompiler) - Maintaining MBINCompiler and creating up-to-date mapping files

## Dependencies

* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) - Handle JSON objects
* [Octokit](https://www.nuget.org/packages/Octokit/) - Query MBINCompiler release information
