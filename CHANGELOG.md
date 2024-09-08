# CHANGELOG

All notable changes to this project will be documented in this file. It uses the
[Keep a Changelog](http://keepachangelog.com/en/1.0.0/) principles and [Semantic Versioning](https://semver.org/)
since 1.0.0.

## Unreleased

### Known Issues
### Added
### Changed
### Deprecated
### Removed
### Fixed
### Security

## 0.13.2 (2024-09-08)

### Changed
* Make `UpdateAsync` awaitable

### Fixed
* Unhandled exception when updating but not internet connection

## 0.13.1 (2024-09-04)

### Added
* New dedicated methods as alternative for `GetMappedKeyOrInput` which works in both directions

## 0.13.0 (2024-08-06)

### Added
* New method `GetMappedKeyOrInput` to map a single key

### Changed
* Existing *mapping.json* is now loaded when changing the `Settings` and therefore the property has been replaced by a getter and a setter

## 0.12.1 (2024-07-22)

### Changed
* Bump *Octokit* from 10.0.0 to 13.0.1
* Updated mapping to 5.01.0.1

## 0.12.0 (2024-04-01)

### Added
* Some more legacy mappings
* `Deobfuscate` and `Obfuscate` have new overloads with the new parameter `useAccount`

### Changed
* The mappings for account data and actual saves are now separated

## 0.11.0 (2024-03-13)

### Added
* Some legacy mappings for account data

### Changed
* Renamed `Download` setting to `DownloadDirectory` to improve clarity
* Settings now have `{ get; set; }` for all targets

## 0.10.1 (2024-03-10)

### Added
* Some legacy mappings

### Changed
* Bump *Octokit* from 9.1.2 to 10.0.0

## 0.10.0 (2024-02-15)

### Added
* Now targeting .NET 8 as per the [.NET release lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
* A `Version` property for the used mapping (downloaded if exists or built-in)
* A new setting `IncludePrerelease` to decide whether pre-releases should be included
  in the mapping updates

### Changed
* Bump *Octokit* from 7.1.0 to 9.1.2
* Updated mapping to 4.50.0.1

## 0.9.2 (2023-09-05)

### Changed
* Bump *Octokit* from 6.0.0 to 7.1.0
* Updated mapping to 4.43.0.1

### Fixed
* A crash if creating the map in multiple threads simultaneously
* A crash if downloading the latest mapping file does not work due to connection
  issues

## 0.9.1 (2023-06-22)

### Changed
* Updated mapping to 4.34.0.1

## 0.9.0 (2023-03-12)

### Added
* Now targeting .NET 7 as per the [.NET release lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
* Now publishing to [NuGet Gallery](https://www.nuget.org/packages/libNOM.map)

### Changed
* Updated mapping to 4.12.1.1

### Fixed
* Crash if *mapping.json* file is in use

## 0.8.3 (2022-10-21)

### Changed
* Updated mapping to 4.4.0.3

## 0.8.2 (2022-10-09)

### Changed
* Updated mapping to 4.0.0.2

### Fixed
* Crash if parent already has a child with the new name

## 0.8.1 (2022-08-10)

### Changed
* Now targeting .NET Standard 2.x and currently supported versions in the [.NET release lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
* Updated mapping to 3.98.0.5

## 0.8.0 (2022-07-25)

### Changed
* Deobfuscate and Obfuscate now take JToken to allow doing so in any node

### Removed
* .NET Framework as explicit target as .NET Standard is enough

## 0.7.0 (2022-05-03)

### Changed
* Renamed `PathDownload` setting to `Download`

## 0.6.2 (2022-05-02)

### Added
* Multiple target frameworks

### Changed
* Updated mapping to 3.88.0.2

## 0.6.1 (2022-04-27)

### Fixed
* Crash when GitHub rate limit (60 requests per hour for unauthenticated requests)
  is exceeded

## 0.6.0 (2022-04-05)

### Changed
* Removed singleton and now use static class instead

## 0.5.0 (2022-04-03)

### Changed
* Hid `UpdateTask` from public and only use internal
* Moved `SetSettings` directly into property

### Fixed
* A deadlock when calling `Update` or `UpdateAsync`

## 0.4.1 (2022-03-04)

### Added
* More output in the release assets
    * NuGet package
    * archive with all necessary DLLs

### Changed
* Updated mapping to 3.82.0.2

## 0.4.0 (2022-02-24)

### Changed
* Updated mapping to 3.81.0.2
* Moved all public classes to root namespace

## 0.3.0 (2022-02-20)

### Added
* `MappingSettings`

## 0.2.1 (2022-01-08)

### Changed
* Removed Serilog dependency

## 0.2.0 (2022-01-03)

### Added
* `UpdateAsync`

## 0.1.1 (2021-12-28)

### Changed
* Using null-forgiving operator instead of disabled warnings
* Structure optimization

## 0.1.0 (2021-12-27)

### Added
* `Deobfuscate`
* `Obfuscate`
* Update mapping by downloading from [latest MBINCompiler release](https://github.com/monkeyman192/MBINCompiler/releases/latest)
