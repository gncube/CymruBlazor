using Xunit;
using Shouldly;
using Bunit;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Accessibility;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Enums;
using CymruBlazor.Accessibility.Notifications;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Components.Accessibility;

public sealed class CyLiveRegionTests : TestContextBase
{
    public CyLiveRegionTests()
    {
        Services.AddScoped<ILiveRegionRegistry, LiveRegionRegistry>();
    }

    [Fact]
    public void Should_Render_With_Default_Accessibility_Attributes()
    {
        // Act
        var cut = Render<CyLiveRegion>();

        // Assert
        var element = cut.Find("*");
        element.GetAttribute("aria-live").ShouldBe("polite");
        element.GetAttribute("aria-atomic").ShouldBe("true");
        element.ClassList.ShouldContain("cy-live-region");
    }

    [Theory]
    [InlineData(LiveRegionPoliteness.Assertive, "assertive")]
    [InlineData(LiveRegionPoliteness.Off, "off")]
    public void Should_Respect_Politeness_Parameter_Changes(LiveRegionPoliteness politeness, string expectedAttr)
    {
        // Act
        var cut = Render<CyLiveRegion>(p => p.Add(c => c.Politeness, politeness));

        // Assert
        cut.Find("*").GetAttribute("aria-live").ShouldBe(expectedAttr);
    }

    [Fact]
    public async Task Should_Update_Dom_Content_When_Mediator_Announcement_Received()
    {
        // Arrange
        var cut = Render<CyLiveRegion>();

        // Act
        await cut.InvokeAsync(async () =>
            await cut.Instance.Handle(new LiveRegionAnnouncement("Operation Successful", LiveRegionPoliteness.Assertive), CancellationToken.None));

        // Assert
        var element = cut.Find("*");
        element.TextContent.ShouldContain("Operation Successful");
        element.GetAttribute("aria-live").ShouldBe("assertive");
    }

    [Fact]
    public async Task Should_Not_Throw_And_Should_Update_Dom_When_Published_Through_Real_Mediator_Pipeline()
    {
        // Arrange - uses the real AddCymruBlazor() DI registration (real
        // Mediator dispatch, not a direct method call) to guard against
        // CyLiveRegion implementing INotificationHandler<> directly,
        // which causes Mediator to resolve a separate, render-handle-less
        // instance from the container and throw
        // "The render handle is not yet assigned."
        Services.AddCymruBlazor();

        var cut = Render<CyLiveRegion>();
        var mediator = cut.Services.GetRequiredService<IMediator>();

        // Act
        await cut.InvokeAsync(async () =>
            await mediator.Publish(new LiveRegionAnnouncement("Code copied to clipboard.", LiveRegionPoliteness.Polite)));

        // Assert
        var element = cut.Find("*");
        element.TextContent.ShouldContain("Code copied to clipboard.");
    }
}
