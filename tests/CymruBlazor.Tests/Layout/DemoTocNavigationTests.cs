using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using CymruBlazor.Demo.Layout;
using CymruBlazor.Demo.Localisation;
using CymruBlazor.Demo.Pages;
using CymruBlazor.Demo.Pages.Components.Forms;
using Mediator;
using Moq;

namespace CymruBlazor.Tests.Layout;

/// <summary>
/// Verifies the Table of Contents anchor links, verifying semantic anchor elements,
/// target ID matching, presence and uniqueness in component pages and home, and fragment routing sync.
/// </summary>
public sealed class DemoTocNavigationTests : TestContextBase
{
    private readonly AppStrings _strings = new();

    public DemoTocNavigationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new Mock<IMediator>().Object);
        Services.AddSingleton(_strings);
    }

    [Fact]
    public void DemoToc_Should_Render_All_Three_Navigation_Links_As_Semantic_Anchors()
    {
        // Act
        var cut = Render<DemoToc>();

        // Assert: Nav element with accessible label
        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").ShouldBe(_strings.Nav.TableOfContents);

        // Assert: Semantic anchors with expected fragment targets
        var links = cut.FindAll("nav ul li a").ToList();
        links.Count.ShouldBe(3);

        var hrefs = links.Select(a => a.GetAttribute("href")).ToList();
        hrefs.ShouldBe(["#examples", "#api", "#accessibility"]);

        foreach (var link in links)
        {
            link.TextContent.Trim().ShouldNotBeNullOrWhiteSpace();
            link.TagName.ToLowerInvariant().ShouldBe("a");
        }
    }

    [Fact]
    public void CyButtonPage_Should_Contain_All_Target_IDs_Simultaneously_And_Uniquely()
    {
        // Act
        var cut = Render<CyButtonPage>();

        // Assert: All three targets exist in the same component render tree
        var examplesElement = cut.Find("#examples");
        var apiElement = cut.Find("#api");
        var a11yElement = cut.Find("#accessibility");

        examplesElement.ShouldNotBeNull();
        apiElement.ShouldNotBeNull();
        a11yElement.ShouldNotBeNull();

        // Assert: Target IDs are unique across the rendered page
        cut.FindAll("[id='examples']").Count.ShouldBe(1);
        cut.FindAll("[id='api']").Count.ShouldBe(1);
        cut.FindAll("[id='accessibility']").Count.ShouldBe(1);
    }

    [Fact]
    public void CyButtonPage_Should_Synchronize_Active_Tab_From_Initial_Url_Fragment()
    {
        // Arrange
        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo("http://localhost/forms/button#api");

        // Act
        var cut = Render<CyButtonPage>();

        // Assert: API tab button selected and active
        var apiTab = cut.Find("#tab-api");
        apiTab.GetAttribute("aria-selected").ShouldBe("true");
        apiTab.ClassList.ShouldContain("active");

        // Assert: API panel is visible (not hidden)
        var apiPanel = cut.Find("#api");
        apiPanel.HasAttribute("hidden").ShouldBeFalse();

        // Assert: Inactive panels are hidden
        var examplesPanel = cut.Find("#examples");
        examplesPanel.HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void CyButtonPage_Should_Switch_Active_Tab_On_LocationChanged_Fragment()
    {
        // Arrange
        var navManager = Services.GetRequiredService<NavigationManager>();
        var cut = Render<CyButtonPage>();

        // Act: Navigate to accessibility fragment
        navManager.NavigateTo("http://localhost/forms/button#accessibility");

        // Assert: Accessibility tab becomes selected and panel visible
        cut.WaitForAssertion(() =>
        {
            var a11yTab = cut.Find("#tab-accessibility");
            a11yTab.GetAttribute("aria-selected").ShouldBe("true");
            a11yTab.ClassList.ShouldContain("active");

            var a11yPanel = cut.Find("#accessibility");
            a11yPanel.HasAttribute("hidden").ShouldBeFalse();
        });
    }

    [Fact]
    public void HomePage_Should_Contain_All_Target_IDs_Uniquely()
    {
        // Act
        var cut = Render<Home>();

        // Assert: Targets exist
        var examples = cut.Find("#examples");
        var api = cut.Find("#api");
        var a11y = cut.Find("#accessibility");

        examples.ShouldNotBeNull();
        api.ShouldNotBeNull();
        a11y.ShouldNotBeNull();

        // Assert: Unique
        cut.FindAll("[id='examples']").Count.ShouldBe(1);
        cut.FindAll("[id='api']").Count.ShouldBe(1);
        cut.FindAll("[id='accessibility']").Count.ShouldBe(1);
    }
}
