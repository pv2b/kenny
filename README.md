# Kenny

Kenny is a solution for using Password Manager Pro as a back-end for Royal TS
documents using Dynamic Folder and Dynamic Credential technology.

With Kenny, [logins](https://www.youtube.com/watch?v=yK0P1Bk8Cx4) to managed
servers is a one-step process.

## Building / publishing

The main application is KennyApi. To build it for installation, simply `cd`
into the KennyApi directory and run `dotnet publish`.

The files in KennyApi\bin\Debug\net6.0 need to be copied to server.

## Installing

### Create Active Directory account

An Active Directory account needs to be created in the domain for the service.
The service must has an SPN associated to it like this:

`setspn -S HTTP/kenny.contoso.com contoso\svc.kenny`

Make sure that the SPN matches the FQDN of the API server.

### Copy files

After building KennyApi, grab the files out of KennyApi\bin\Debug\net6.0
and put them on the server. In this example, I will put them in C:\Kenny

After building KennyCrawler, grab the files out of KennyCrawler\bin\Debug\net6.0
and put them on the server. In this example, I will put them in C:\KennyCrawler

### Log on as service permissions

To establish Log on as a service rights for a service user account:

1. Open the Local Security Policy editor by running secpol.msc.
2. Expand the Local Policies node and select User Rights Assignment.
3. Open the Log on as a service policy.
4. Select Add User or Group.
5. Provide the object name (user account) using either of the following approaches:
    - Type the user account ({DOMAIN OR COMPUTER NAME\USER}) in the object name field and select OK to add the user to the policy.
    - Select Advanced. Select Find Now. Select the user account from the list. Select OK. Select OK again to add the user to the policy.
6. Select OK or Apply to accept the changes.

### Add event log source

This has to be run as an admin:

    New-EventLog -Source KennyApi -LogName Application

### SSL Certificate

Kenny can use a SSL certificate enrolled to the machine by configuring appsettings.json like this:

    {
        "Logging": {
            "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "Kestrel": {
        "Endpoints": {
            "Https": {
                "Url": "https://0.0.0.0:5000",
                "Certificate": {
                    "Subject": "kenny.contoso.com",
                    "Store": "My",
                    "Location": "LocalMachine",
                    "AllowInvalid": "true"
                    }
                }
            }
        }
    }

You will need to grant the service account read permissions to the private key.
(Use the MMC snap-in for certificates, pointed at the local machine, and grant
read permissions to the private key to the service account.)

### Set file permissions and create service

The following Powershell snippet will set the appopriate file permissions and create the service. Make sure to edit the variable definitions at the top!

    $app_path = "C:\Kenny"
    $service_user = "contoso\svc.kenny"
    
    $exe_path = Join-Path $app_path "KennyApi.exe"
    $acl = Get-Acl "$app_path"
    $aclRuleArgs = $service_user, "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
    $acl.SetAccessRule($accessRule)
    $acl | Set-Acl "$app_path"

    New-Service -Name "Kenny" -BinaryPathName (Join-Path $app_path "KennyApi.exe") -Credential $service_user -Description "Kenny integrates Royal TS with Password Manager Pro" -DisplayName "Kenny" -StartupType Automatic

### Add API keys and authorize them

Adding API keys happens by creating a file called `ApiKeyring.json` in the C:\Kenny`. folder
(or wherever Kenny has been installed.)

The file should look like this:

    {
        "Contoso": {
            "ApiBaseUri": "https://pmpserver.contoso.com/",
            "ApiAuthToken": "SecureLuggage12345",
            "AllowGroups": [ "CONTOSO\\Nice People", "CONTOSO\\Also Nice People" ],
            "DenyGroups": [ "CONTOSO\\Naughty People", "CONTOSO\\Very Naughty People" ]
            "AllowUsers": [ "CONTOSO\\goodguy1", "CONTOSO\\goodguy2" ]
            "DenyUsers": [ "CONTOSO\\badguy1", "CONTOSO\\badguy2" ]
        }
        "Northwind": {
            "ApiBaseUri": "https://pmpserver.contoso.com/",
            "ApiAuthToken": "PasswordForAChocolateBar",
            "DenyUsers": [ "NORTHWIND\\hackerman" ]
        }
    }
    
The keys correspond to the `collection` parameter on the API calls. This
is used to determine what API Key (`AuthToken`) from the keyring is to be
used for that collection.

`ApiBaseUri` (mandatory) determines the location of the Password Manager Pro
server.

`ApiAuthToken` (mandatory) is the authentication token belinging to the
Password Manager Pro API user.

`AllowGroups` (optional) is a list of groups that are authorized to use this
API key.

`DenyGroups` (optional) is a list of groups that are authorized to use this
API key.

`AllowUsers` (optional) is a list of users that are authorized to use this
API key.

`DenyUsers` (optional) is a list of users that are authorized to use this
API key.

Deny takes precedence over allow. If both `AllowGroups` and `AllowUsers`
are empty or missing, nobody is allowed in. (I.e. there's no default
"allow all" behaviour.)

Note that JSON requires backslashes to be escaped by doubling them inside
strings.

## KennyCrawler scheduled task

Create scheduled task for KennyCrawler to run as Kenny service account.
This can be done manually using task scheduler.

IMPORTANT: Make the task have its working directory in C:\Kenny folder
(where API is) NOT in C:\KennyCrawler folder!

## Royal TS Configuration

Add a dynamic folder with the following Dynamic Folder Script (Powershell):

    (Invoke-WebRequest -UseDefaultCredentials -Method Get -Uri https://kenny.contoso.com:5000/DynamicFolder?collection=contoso).Content

... and the following Dynamic Credential Script (Powershell):

    $ResourceId = @'
    $Target.CustomField1$
    '@

    $CredentialId = @'
    $DynamicCredential.EffectiveID$
    '@

    $AccountId = $CredentialId -replace "^PmpCred_", ""

    (Invoke-WebRequest -UseDefaultCredentials -Method Get -Uri "https://kenny.contoso.com:5000/DynamicCredential?collection=contoso&resourceId=$ResourceId&accountId=$AccountId").Content

You will need to edit the script to edit the hostname of the Kenny server and
the collection name.