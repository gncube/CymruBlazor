using System.Net;
using CymruBlazor.Services;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests;

public class NuGetPackageVersionServiceTests
{
    [Fact]
    public async Task GetLatestVersionAsync_WithNoHttpClient_ReturnsNull()
    {
        // Arrange
        var service = new NuGetPackageVersionService();

        // Act
        var result = await service.GetLatestVersionAsync("CymruBlazor");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetLatestVersionAsync_ExcludingPrerelease_ReturnsLatestStable()
    {
        // Arrange
        using var httpClient = CreateHttpClient(
            """{"versions":["0.1.0-preview.1","1.0.0","1.1.0-preview.1"]}""");
        var service = new NuGetPackageVersionService(httpClient);

        // Act
        var result = await service.GetLatestVersionAsync("CymruBlazor", includePrerelease: false);

        // Assert
        result.ShouldBe("1.0.0");
    }

    [Fact]
    public async Task GetLatestVersionAsync_IncludingPrerelease_ReturnsLastListedVersion()
    {
        // Arrange
        using var httpClient = CreateHttpClient(
            """{"versions":["1.0.0","1.1.0-preview.1"]}""");
        var service = new NuGetPackageVersionService(httpClient);

        // Act
        var result = await service.GetLatestVersionAsync("CymruBlazor", includePrerelease: true);

        // Assert
        result.ShouldBe("1.1.0-preview.1");
    }

    [Fact]
    public async Task GetLatestVersionAsync_NoStableVersionYet_FallsBackToPrerelease()
    {
        // Arrange
        using var httpClient = CreateHttpClient(
            """{"versions":["0.1.0-preview.1","0.2.0-preview.1"]}""");
        var service = new NuGetPackageVersionService(httpClient);

        // Act
        var result = await service.GetLatestVersionAsync("CymruBlazor", includePrerelease: false);

        // Assert
        result.ShouldBe("0.2.0-preview.1");
    }

    [Fact]
    public async Task GetLatestVersionAsync_RequestFails_ReturnsNull()
    {
        // Arrange
        using var httpClient = CreateHttpClient(
            content: null,
            statusCode: HttpStatusCode.NotFound);
        var service = new NuGetPackageVersionService(httpClient);

        // Act
        var result = await service.GetLatestVersionAsync("does-not-exist");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetLatestVersionAsync_NullOrWhitespacePackageId_Throws()
    {
        // Arrange
        var service = new NuGetPackageVersionService();

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            () => service.GetLatestVersionAsync(" "));
    }

    private static HttpClient CreateHttpClient(string? content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new StubHttpMessageHandler(content, statusCode);
        return new HttpClient(handler);
    }

    private sealed class StubHttpMessageHandler(string? content, HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = content is null ? null : new StringContent(content)
            };

            return Task.FromResult(response);
        }
    }
}
