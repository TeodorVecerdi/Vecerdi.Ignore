# Vecerdi.Ignore

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Vecerdi.Ignore)


Generate `.gitignore` files for your projects based on one or more templates from [gitignore.io](https://gitignore.io).

## Installation

```bash
dotnet tool install -g Vecerdi.Ignore
```

## Usage

```bash
$ ignore --help

Description:
  Generate a .gitignore file based on one or more gitignore.io templates

Usage:
  ignore <ignores>... [command] [options]

Arguments:
  <ignores>  One or more gitignore.io templates to use

Options:
  -o, --output <file>  Set the output file to write the .gitignore to [default: .gitignore]
  --version            Show version information
  -?, -h, --help       Show help and usage information


Commands:
  list  List all available templates
```

### Example
```bash
$ ignore rider,unity
$ ignore rider unity --output .gitignore
```
