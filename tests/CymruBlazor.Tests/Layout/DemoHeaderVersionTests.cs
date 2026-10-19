using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using CymruBlazor.Demo.Layout;
using CymruBlazor.Demo.Localisation;
using CymruBlazor.Demo.Services;
using CymruBlazor.Services;
using CymruBlazor.Themes;

namespace CymruBlazor.Tests.Layout;

/// <summary>
/// Verifies that the header version badge renders data-driven version metadata
/// resolving from the authoritative CymruVersionProvider, correctly handles
/// stable and prerelease SemVer formatting without duplicating 'v', and gracefully
/// handles missing version information.
/// </summary>
public sealed class DemoHeaderVersionTests : TestContextBase
{
    public DemoHeaderVersionTests()
    {
        Services.AddSingleton<IThemeService>(new ThemeService());
        Services.AddSingleton(new AppStrings());
    }

    [Fact]
    public void Authoritative_Version_Provider_Should_Resolve_Non_Empty_Version()
    {
        var version = CymruVersionProvider.Version;

        version.ShouldNotBeNullOrWhiteSpace();
        version.ShouldStartWith("v");
    }

    [Theory]
    [InlineData("1.0.0", "v1.0.0")]
    [InlineData("1.3.0", "v1.3.0")]
    [InlineData("2.0.0", "v2.0.0")]
    [InlineData("2.1.0+commit123", "v2.1.0")]
    public void FormatVersion_Should_Format_Stable_Versions(string input, string expected)
    {
        CymruVersionProvider.FormatVersion(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.4.0-preview.1", "v1.4.0-preview.1")]
    [InlineData("2.0.0-rc.2", "v2.0.0-rc.2")]
    [InlineData("2.0.0-alpha.5+sha7738", "v2.0.0-alpha.5")]
    public void FormatVersion_Should_Preserve_Prerelease_Suffixes(string input, string expected)
    {
        CymruVersionProvider.FormatVersion(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("v1.3.0", "v1.3.0")]
    [InlineData("V1.3.0", "v1.3.0")]
    [InlineData("v1.4.0-preview.1", "v1.4.0-preview.1")]
    public void FormatVersion_Should_Not_Duplicate_Prefix(string input, string expected)
    {
        CymruVersionProvider.FormatVersion(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+metadataOnly")]
    public void FormatVersion_Should_Return_Null_For_Missing_Or_Invalid_Input(string? input)
    {
        CymruVersionProvider.FormatVersion(input).ShouldBeNull();
    }

    [Fact]
    public void DemoHeader_Should_Render_Authoritative_Version_By_Default()
    {
        // Act
        var cut = Render<DemoHeader>();

        // Assert
        var badge = cut.Find(".cb-version-badge");
        badge.TextContent.Trim().ShouldBe(CymruVersionProvider.Version);
    }

    [Fact]
    public void DemoHeader_Should_Render_Formatted_Stable_Version_When_Specified()
    {
        // Act
        var cut = Render<DemoHeader>(parameters => parameters.Add(p => p.Version, "1.3.0"));

        // Assert
        var badge = cut.Find(".cb-version-badge");
        badge.TextContent.Trim().ShouldBe("v1.3.0");
    }

    [Fact]
    public void DemoHeader_Should_Preserve_Prerelease_Suffix_When_Specified()
    {
        // Act
        var cut = Render<DemoHeader>(parameters => parameters.Add(p => p.Version, "1.4.0-preview.1"));

        // Assert
        var badge = cut.Find(".cb-version-badge");
        badge.TextContent.Trim().ShouldBe("v1.4.0-preview.1");
    }

    [Fact]
    public void DemoHeader_Should_Not_Duplicate_Prefix_When_Already_Supplied()
    {
        // Act
        var cut = Render<DemoHeader>(parameters => parameters.Add(p => p.Version, "v1.3.0"));

        // Assert
        var badge = cut.Find(".cb-version-badge");
        badge.TextContent.Trim().ShouldBe("v1.3.0");
    }

    [Fact]
    public void DemoHeader_Should_Omit_Badge_When_Version_Is_Empty()
    {
        // Act
        var cut = Render<DemoHeader>(parameters => parameters.Add(p => p.Version, ""));

        // Assert
        cut.FindAll(".cb-version-badge").ShouldBeEmpty();
    }

    [Fact]
    public void DemoHeader_Search_Trigger_Should_Follow_Label_In_Name_Guidelines()
    {
        // Act
        var cut = Render<DemoHeader>();

        // Assert
        var searchBtn = cut.Find("button.cb-shell-header__search-trigger");
        searchBtn.ShouldNotBeNull();

        // Must not carry an overriding aria-label that mismatches visible content
        searchBtn.HasAttribute("aria-label").ShouldBeFalse();

        // Keyboard shortcut hint must be aria-hidden
        var kbd = searchBtn.QuerySelector(".search-kbd");
        kbd.ShouldNotBeNull();
        kbd.GetAttribute("aria-hidden").ShouldBe("true");

        // Visible search text should match AppStrings.Nav.Search
        var searchText = searchBtn.QuerySelector(".search-text");
        searchText.ShouldNotBeNull();
        searchText.TextContent.Trim().ShouldBe("Search");
    }
}
