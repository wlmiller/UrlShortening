# URL Shortening

A simple URL shortening service using .NET and Azure.

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/wlmiller/UrlShortening/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/5gk62oq3v8e8c310?svg=true)](https://ci.appveyor.com/project/wlmiller/urlshortening)

## Deployment

Prior to deployment, `ng build --prod` must be run in [the frontend app folder](Durwella.UrlShortening.Web/Durwella.UrlShortening.Web/ClientApp/UrlShortening) to compile and webpack the Angular typescript code.

You should also have an existing Azure Storage Account. Once deployed you can add or modify the connection string for `AzureStorage`. 

1. Click **Manage** when deployment is finished (or browse to the web app on the [Azure Portal](https://portal.azure.com)
1. Click **Settings**
1. Select **Application settings**
1. Click **Show Connection Strings**
1. Add a new **Custom** connection string named **AzureStorage** and paste the connection string to an Azure Storage Account

If you do not provide the Azure Storage connection string an in-memory URL Alias repository will be used. 
In that case short URLs will break when the web app is restarted.

Also, a client secret is required for securely signing JWT tokens (for information about generating a secret key, see [this introduction](https://github.com/dwyl/learn-json-web-tokens#how-to-generate-secret-key)) This can also be set on the **Application settings** screen in the Azure Portal.

1. Add a new **Application setting** named **Auth:Secret** with a value set to your client secret.

## Architecture

[UrlShortener](Durwella.UrlShortening/UrlShortener.cs) contains the core logic. 
There are interfaces you can use to customize the specific mechanics:

- [IHashScheme](Durwella.UrlShortening/IHashScheme.cs) - Used to generate the short URL. Should generate an appropriate short "hash" from a long string. A [default](Durwella.UrlShortening/Sha1Base64Scheme.cs) implementation is provided which is based on Base64 encoding of SHA1. Implementations must also provide a way to iteratetively generate new hashes from the same input in the case of a hash collision.
- [IAliasRepository](Durwella.UrlShortening/IAliasRepository.cs) - Dictionary-like persistence of "alias" or "hash" of one string (the key) to another (the value). Used to save mapping between short and long URLs. The [default](Durwella.UrlShortening/AzureTableAliasRepository.cs) uses Azure Table storage. The [fallback](Durwella.UrlShortening/MemoryAliasRepository.cs) uses an in-memory Dictionary.
- [IUrlUnwrapper](Durwella.UrlShortening/IUrlUnwrapper.cs) - Responsible for resolving a direct URL to a resource. For example the provided URL might already be a 'short URL', which could lead to multiple redirects or a redirect loop. The [default](Durwella.UrlShortening/WebClientUrlUnwrapper.cs) uses [WebClient](https://msdn.microsoft.com/en-us/library/system.net.webclient).

## About this Fork

This is a fork of [Durwella.UrlShortening](https://github.com/durwella/urlshortening).
The original version uses [ServiceStack](https://servicestack.net/) on .NET Framework 4.5.1, with an AngularJS frontend.
In this fork, the project has been convered to an ASP.NET Core 2.0 project (removing ServiceStack entireley), with an Angular 5 frontend (using Angular CLI). All libraries and test projects were converted to .NET Standard 2.0.

The intent in the conversion was to maintain the original functionality, but there are a few changes worth noting. Most of these changes were just to more closely fit my personal use case.

- By default, the root path serves an admin page, just as in the upstream repo. However, if a `RedirectUrl` is specified in application aettings, the root path will serve a `302 Moved` redirect to the specified URL. The admin page can be accessed at `/admin`.
- Authentication changes:
    * Azure Active Directory support wasn't poprted - only "credentials" authentication is supported.
    * There is no option for unauthenticated short url creation.
    * Authorization is via [JSON Web Tokens](https://jwt.io) instead of cookies. The server requires a client secret to generate secure JWTs, which must be specified in the application settings under the key `Auth:Secret`. The tokens expire after an hour by default, but that length can be changed with the `Auth:TokenExpirationMinutes` app settings.
- The title and logo on the admin page can no longer be specified via application settings, and unauthenticated visitors to the admin page will be presented with a login popup immediately.
- In the upstream version, creating a short url using a custom path for a destination that already had a short url defined would replace the original one. This would cause existing links using the "old" url to break, so this has been changed so that a destination url could potentially have zero or one "hash" short urls and arbitrarily-many custom ones. When a short url is requested for a destination that already exists in the repository, a random short url (whichever one is returned by `First()` from Azure Table Storage) will be returned.
- `azuredeploy.json` was removed. For this fork, an additional step (`ng build`) is required to compile the Angular application before deployment of the application. It could probably be accomplished by including the result of `ng build` in [wwwroot](Durwella.UrlShortening.Web/Durwella.UrlShortening.Web/wwwroot), but I haven't investigated.

## Contributing

- Natural enhancements to this project would be: 
	- Other persistence options implementing [IAliasRepository](Durwella.UrlShortening/IAliasRepository.cs)
	- Other hashing schemes implementing [IHashScheme](Durwella.UrlShortening/IHashScheme.cs)
- Core logic in the Durwella.UrlShortening project must be unit tested.
- Unit tests should be written using 3 paragaphs corresponding to [Arrange, Act and Assert](http://c2.com/cgi/wiki?ArrangeActAssert)
- Build must remain clean (no warnings, tests passing)
- Code Analysis issues should not be introduced

## References

[URL Shortening: Hashes In Practice](http://blog.codinghorror.com/url-shortening-hashes-in-practice/)  
Nice article by Jeff Atwood. Historical yet relevant.
