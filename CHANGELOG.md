# CHANGELOG

All notable changes to this project will be documented in this file. It uses the
[Keep a Changelog](http://keepachangelog.com/en/1.0.0/) principles and
[Semantic Versioning](https://semver.org/).

## Unreleased

### Added
### Changed
### Deprecated
### Removed
### Fixed
### Security

## 0.8.0 (2022-07-25)

### Changed
* Deobfuscate and Obfuscate now take JToken to allow doing so in any subnode

### Removed
* .NET Framework as explicit target as .NET Standard is enough

## 0.7.0 (2022-05-03)

### Changed
* Renamed some settings

## 0.6.2 (2022-05-02)

### Added
* Multiple target frameworks

### Changed
* Updated mapping to 3.88.0.2

## 0.6.1 (2022-04-27)

### Fixed
* Crash when GitHub rate limit (60 requests per hour for unauthenticated requests) is exceeded

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
* MappingSettings

## 0.2.1 (2022-01-08)

### Changed
* Removed Serilog dependency

## 0.2.0 (2022-01-03)

### Added
* UpdateAsync

## 0.1.1 (2021-12-28)

### Changed
* Using null-forgiving operator instead of disabled warnings
* Structure optimization

## 0.1.0 (2021-12-27)

### Added
* Deobfuscate
* Obfuscate
* Update mapping by downloading from [lastest MBINCompiler release](https://github.com/monkeyman192/MBINCompiler/releases/latest)
