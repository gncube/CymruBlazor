namespace CymruBlazor.Tests.Accessibility;

using Bunit;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Accessibility;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

public sealed class CyLiveRegionTests : BunitContext
{
    private readonly Mock<ILiveRegionRegistry> _registryMock = new();

    public CyLiveRegionTests()
    {
        Services.AddSingleton(_registryMock.Object);
    }

    [Fact]
    public void WhenRenderedLiveRegionContainsExpectedAccessibilityAttributesAndVisuallyHiddenClasses()
    {
        var cut = Render<CyLiveRegion>();

        var div = cut.Find("div");
        div.ClassList.ShouldContain("cy-live-region");
        div.ClassList.ShouldContain("cy-visually-hidden");
        div.GetAttribute("aria-live").ShouldBe("polite");
        div.GetAttribute("aria-atomic").ShouldBe("true");
        div.GetAttribute("aria-relevant").ShouldBe("additions text");
    }

    [Fact]
    public void WhenAnnouncementHandledTextRemainsInDomForAssistiveTechnology()
    {
        var cut = Render<CyLiveRegion>();

        cut.InvokeAsync(() => cut.Instance.Handle(
            new CymruBlazor.Accessibility.Notifications.LiveRegionAnnouncement(
                "Code copied to clipboard.",
                CymruBlazor.Enums.LiveRegionPoliteness.Polite),
            CancellationToken.None));

        var div = cut.Find("div");
        div.TextContent.Trim().ShouldBe("Code copied to clipboard.");
        div.ClassList.ShouldContain("cy-visually-hidden");
    }
}
