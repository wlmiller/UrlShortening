FROM microsoft/dotnet:2.0-sdk

COPY Durwella.UrlShortening Durwella.UrlShortening
COPY Durwella.UrlShortening.Tests Durwella.UrlShortening.Tests

WORKDIR Durwella.UrlShortening.Tests
CMD ["dotnet", "test"]