Retrieve a Bearer token and save into a file for later use.

### To get a new token, remove token file.

## Configuration file for connecting an Azure resource
The applicaion needs `crm_connection.json` with these keys to connect an Azure resource:

```json
{
  "Authority": "https://login.microsoftonline.com/with the tenant id",
  "ClientId": "the clientId of application obtained from Azure portal",
  "ClientSecret": "the client secret of application obtained from Azure portal",
  "Resource": "url of resource to access: e.g.: https://ersaltd.crm6.dynamics.com"
}
```

## Notes:

This application is meant to be used to get a Bearer token of an Azure account for other applications,
which do not have access of the creditials of this Azure account (mainly service account), to use. Once
you have a cache file, copy over to other applications which use `AzureTokenCache.FileCahce` and
tokens should be refreshed when it needs.