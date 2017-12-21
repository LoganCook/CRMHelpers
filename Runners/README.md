# Scripts or applications to run between AD and MS Dynamics

The project needs these packages and projects in this solution as dependencies:

```shell
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Settings.Configuration
dotnet add package Novell.Directory.Ldap.NETStandard --version 2.3.8
dotnet add package System.DirectoryServices --version 4.5.0-preview2-25707-02 --source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
dotnet add .\Runners\Runners.csproj reference .\ADConnect\ADConnect.csproj
dotnet add .\Runners\Runners.csproj reference .\AzureTokenCache\AzureTokenCache.csproj
```

## Configuration files
### Main configuration
The application needs a configuration file in JSON format (default path is `secrets.json`). It has three sections:
1. AD
2. MS Dynamics
3. Notifiers

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

### Log configuration
The application logs its activities to files. To configure logs, use [`log.json.example`](log.json.example) as
a template and modify the values. For more choices of configuring logs, reference [Serilog.Sink.File](https://github.com/serilog/serilog-sinks-file).


## Deployment on Linux
In this example, CentOS is the targeting system.

With building environment and source code, run the command to create files for deployment:

`dotnet publish -c Release -r centos.7-x64`

The resulting files are in: `bin\Release\netcoreapp2.0\centos.7-x64\publish`.

If it is the first time, on targeting CentOS system, run:
```shell
 sudo yum install libunwind
```

The application runs in [globalization invariant mode](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/globalization-invariant-mode.md).
With this mode, hosting Linux system does not need to install ICU libraries.

On Linux, the application connect to AD through LDAPS, so in above mentioned `secrets.json`, `UseSSL` has to be `true` but may need `ForceSSL` to be `false`
in case there are certification problems :disappointed:.

```json
{
    "Host": "ad.server",
    "Port": 636,
    "LoginDN": "CN=user,DC=edu,DC=au",
    "Password": "password",
    "UseSSL": true,
    "ForceSSL": false
}
```

If above steps do not work on CentOS, see [dotnet core doc](https://github.com/dotnet/core/blob/0b1a1631593d6d379fbdfe2b23597a5c25ea4fc9/Documentation/build-and-install-rhel6-prerequisites.md).

## Run the application
The application accepts two keyed arguments with these names:

1. `earliest`: Earliest date since when accounts were created in the format of `yyyyMMdd`. Default is from the beginning of current year.
1. `config`: Path relative to current directory of the configuration JSON file. Default is `secrets.json`.

It also needs to have saved Azure token for connecting MS Dynamics: `svc_crm_tokencache.data` which can be obtained
by running project `AzureTokenCache`.

```shell
# in the package directory, for example
./Runners config=other.json earliest=20171201
```
