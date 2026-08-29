using Xunit;
using Shouldly;
using Bunit;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;
using CymruBlazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Tests.Components.Layout;

public sealed class CyFooterTests : TestContextBase
{
    [Fact]
    public void Should_Render_Explicit_Version_Without_Calling_Service()
    {
        // Arrange - a service that would fail the test if it were called
        Services.AddScoped<IPackageVersionService>(_ => new ThrowingPackageVersionService());

        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.ShowVersion, true)
            .Add(p => p.Version, "1.2.3"));

        // Assert
        cut.Find(".cy-footer__version").TextContent.ShouldContain("1.2.3");
    }

    [Fact]
    public void Should_Render_Resolved_Version_When_ShowVersion_And_No_Explicit_Version()
    {
        // Arrange
        Services.AddScoped<IPackageVersionService>(
            _ => new StubPackageVersionService("2.0.0"));

        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.ShowVersion, true));

        // Assert
        cut.Find(".cy-footer__version").TextContent.ShouldContain("2.0.0");
    }

    [Fact]
    public void Should_Not_Render_Version_When_ShowVersion_False()
    {
        // Arrange
        Services.AddScoped<IPackageVersionService>(
            _ => new StubPackageVersionService("2.0.0"));

        // Act
        var cut = Render<CyFooter>();

        // Assert
        cut.FindAll(".cy-footer__version").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Not_Render_Version_When_Service_Not_Registered()
    {
        // Act - no IPackageVersionService registered at all
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.ShowVersion, true));

        // Assert
        cut.FindAll(".cy-footer__version").Count.ShouldBe(0);
    }

    private sealed class StubPackageVersionService(string version) : IPackageVersionService
    {
        public Task<string?> GetLatestVersionAsync(
            string packageId,
            bool includePrerelease = false,
            CancellationToken cancellationToken = default) => Task.FromResult<string?>(version);
    }

    private sealed class ThrowingPackageVersionService : IPackageVersionService
    {
        public Task<string?> GetLatestVersionAsync(
            string packageId,
            bool includePrerelease = false,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "Should not be called when an explicit Version is supplied.");
    }

    [Fact]
    public void Should_Render_Copyright_When_Provided()
    {
        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.Copyright, "(c) 2026 CymruBlazor contributors"));

        // Assert
        cut.Find(".cy-footer__copyright").TextContent.ShouldContain("2026 CymruBlazor contributors");
    }

    [Fact]
    public void Should_Render_Links_When_ChildContent_Provided()
    {
        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .AddChildContent("<a href=\"/privacy\">Privacy</a>"));

        // Assert
        cut.Find(".cy-footer__links a").TextContent.ShouldContain("Privacy");
    }

    [Fact]
    public void Should_Not_Render_Empty_Wrappers_When_Nothing_Provided()
    {
        // Act
        var cut = Render<CyFooter>();

        // Assert
        cut.FindAll(".cy-footer__links").Count.ShouldBe(0);
        cut.FindAll(".cy-footer__copyright").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-footer--primary")]
    [InlineData(ComponentColour.Secondary, "cy-footer--secondary")]
    [InlineData(ComponentColour.Surface, "cy-footer--surface")]
    [InlineData(ComponentColour.Neutral, "cy-footer--neutral")]
    public void Should_Apply_Background_Css_Class(ComponentColour background, string expectedClass)
    {
        // Act
        var cut = Render<CyFooter>(parameters => parameters
            .Add(p => p.Background, background));

        // Assert
        cut.Find(".cy-footer").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Default_To_Primary_Background()
    {
        // Act
        var cut = Render<CyFooter>();

        // Assert
        cut.Find(".cy-footer").ClassList.ShouldContain("cy-footer--primary");
    }

    [Fact]
    public void Should_Reject_Unsupported_Background()
    {
        // Act
        var act = () => Render<CyFooter>(parameters => parameters
            .Add(p => p.Background, ComponentColour.Danger));

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
}
