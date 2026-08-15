using Xunit;
using Shouldly;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Mediator;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Enums;
using CymruBlazor.Accessibility.Notifications;

namespace CymruBlazor.Tests.Components.Accessibility;

public sealed class CyLiveRegionTests : TestContextBase
{
    private readonly Mock<IMediator> _mediatorMock = new();

    public CyLiveRegionTests()
    {
        Services.AddSingleton(_mediatorMock.Object);
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
}
