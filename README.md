# libNOM.map

![Maintained](https://img.shields.io/maintenance/yes/2023)
[![.NET | Standard 2.0 - 2.1 | 6 - 7](https://img.shields.io/badge/.NET-Standard%202.0%20--%202.1%20%7C%206%20--%207-lightgrey)](https://dotnet.microsoft.com/en-us/)
[![C# 11](https://img.shields.io/badge/C%23-11-lightgrey)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Release](https://img.shields.io/github/v/release/zencq/libNOM.map?display_name=tag)](https://github.com/zencq/libNOM.map/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/libNOM.map)](https://www.nuget.org/packages/libNOM.map/)

[![libNOM.map](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml/badge.svg)](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml)

## Introduction

The `libNOM` label is a collection of .NET class libraries originally developed
and used in [NomNom](https://github.com/zencq/NomNom), the most complete savegame
editor for [No Man's Sky](https://www.nomanssky.com/).

`libNOM.map` can be used to obfuscate and deobfuscate the file content.

## Getting Started

The mapping can be accessed through a static class and each functionality is just
a simple call.

Not only the latest mapping is supported but also legacy keys that are gone in a
game version after **Beyond 2.11**. It is also possible to download an updated mapping
file from the [lastest MBINCompiler release](https://github.com/monkeyman192/MBINCompiler/releases/latest).
It will be downloaded to **download/mapping.json** (if no other path is set) and
automatically used if present.

The built-in mapping currently is `4.34.0.1`.

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
