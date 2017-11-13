Common Classes shared between projects

The project needs these packages and projects in this solution as dependencies:

```shell
dotnet add package System.Json
dotnet add .\Commons\Commons.csproj reference .\ADConnect\ADConnect.csproj
```

## Entities namespace
For acting with data in MS Dynamics

## Types namespace
For converting data in/out from MS Dynamics from JSON 