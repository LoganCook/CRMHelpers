# Tests for included projects

### Installed packages

```
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package MSTest.TestFramework
dotnet add package xuint
dotnet add package Moq
```

### Projects are tested

```
dotnet add reference ..\Synchroniser\Synchroniser.csproj
dotnet add reference ..\ADConnect\ADConnect.csproj
dotnet add reference ..\Client\Client.csproj
```