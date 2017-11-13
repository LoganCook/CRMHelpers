Scripts or programs to run between AD and MS Dynamics

The project needs these packages and projects in this solution as dependencies:

```shell
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Novell.Directory.Ldap.NETStandard --version 2.3.8
dotnet add package System.DirectoryServices --version 4.5.0-preview2-25707-02 --source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
dotnet add .\Runners\Runners.csproj reference .\ADConnect\ADConnect.csproj
dotnet add .\Runners\Runners.csproj reference .\AzureTokenCache\AzureTokenCache.csproj
```

## Configure how to connect AD
The application needs `secrets.json` with these keys to connect AD var LDAP and MS Dynamics:

```json
{
  "AD": {
    "Host": "ad.server",
    "Port": 636,
    "LoginDN": "CN=user,DC=edu,DC=au",
    "Password": "password",
    "UseSSL": true,
    "ForceSSL": true
  },
  "Dynamics": {
    "Authority": "https://login.microsoftonline.com/27405d08-6a6f-4442-8954-ab68b0574cd0",
    "ClientId": "uuid",
    "ClientSecret": "good string",
    "Resource": "https://ersasandbox.crm6.dynamics.com",
    "Version": "8.2"
  },
  "Notifiers": {
    "Email": {
      "Server": "smtp-mail.outlook.com",
      "Port": 587,
      "Name": "display name, optional",
      "Username": "outlook.office.com user",
      "Password": "password",
      "Receivers": [ "receiver1" ]
    }
  }
}

```

It also need to have saved Azure token for connecting MS Dynamics: `svc_crm_tokencache.data` which can be obtained
by running project `AzureTokenCache`.

## Run the application
The application accepts two keyed arguments with these names:

1. `earliest`: Earliest date since when accounts were created in the format of `yyyyMMdd`. Default is from the beginning of current year.
1. `config`: Path relative to current directory of the configuration JSON file. Default is `secrets.json`.

`dotnet run config=other.json earliest=20171201`