# libNOM.map

![Maintained](https://img.shields.io/maintenance/yes/2022)
![.NET 6](https://img.shields.io/badge/.NET-6-lightgrey)
![C# 10](https://img.shields.io/badge/C%23-10-lightgrey)
![Release](https://img.shields.io/github/v/release/zencq/libNOM.map?display_name=tag)

[![libNOM.map](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml/badge.svg)](https://github.com/zencq/libNOM.map/actions/workflows/pipeline.yml)

## Introduction

The `libNOM` label is a collection of .NET class libraries originally developed and used
in [NomNom](https://github.com/zencq/NomNom), a savegame editor for [No Man's Sky](https://www.nomanssky.com/).

`libNOM.map` can be used to obfuscate and deobfuscate save file content.

## Getting Started

The mapping is stored in a singleton instance and each function is just a simple call.

Not only the latest mapping is supported but also legacy keys that are gone in a
game version after **2.09**. It is also possible to download an updated the mapping from
the [lastest MBINCompiler release](https://github.com/monkeyman192/MBINCompiler/releases/latest).
It will be downloaded to **download/mapping.json** and automatically loaded if present.

### Requirements

The following packages are used in the public API:
* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) for JSON handling.

### Usage

The obfuscation and deobfuscation is done in-place. Deobfuscation will return a list
of unknown keys.
```csharp
// Deobfuscate
HashSet<string> unknownKeys = Mapping.Instance.Deobfuscate(jsonObject);

// Obfuscate
Mapping.Instance.Obfuscate(jsonObject);
```

Update and download the mapping.json if a newer version is available.
```csharp
// Update
Mapping.Instance.Update()
```

## License

This project is licensed under the GNU GPLv3 license - see the [LICENSE](LICENSE)
file for details.

## Authors

* **Christian Engelhardt** (zencq) - [GitHub](https://github.com/cengelha)

## Credits

Thanks to the following people for their help in one way or another.

* [monkeyman192](https://github.com/monkeyman192/MBINCompiler) - Maintaining MBINCompiler and creating up-to-date mapping files
