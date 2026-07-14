using System;
using CymruBlazor.Services;
using CymruBlazor.Themes;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void DefaultTheme_IsDefault()
    {
        var service = new ThemeService();

        service.Current.ShouldBe(ThemeMode.Default);
    }

    [Fact]
    public void NotifyChanged_IsRaised()
    {
        var service = new ThemeService();
        bool raised = false;
        service.NotifyChanged += (s, e) => raised = true;
        service.Current = ThemeMode.Dark;

        raised.ShouldBeTrue();
        service.Current.ShouldBe(ThemeMode.Dark);
    }
}
