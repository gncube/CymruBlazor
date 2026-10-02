using Xunit;
using Shouldly;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Content;
using CymruBlazor.Diagnostics;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyIconDiagnosticsTests : TestContextBase
{
    [Fact]
    public void Unknown_Name_Throws_When_Strict()
    {
        Services.AddSingleton<ICyDiagnostics>(new CyDiagnostics(new CymruBlazorOptions()));

        Should.Throw<ArgumentException>(() =>
            Render<CyIcon>(p => p.Add(x => x.Name, "no-such-icon")));
    }

    [Fact]
    public void Unknown_Name_Renders_Placeholder_When_Lenient()
    {
        Services.AddSingleton<ICyDiagnostics>(new CyDiagnostics(
            new CymruBlazorOptions { Diagnostics = CyDiagnosticsMode.Lenient }));

        var cut = Render<CyIcon>(p => p.Add(x => x.Name, "no-such-icon"));

        cut.Find("svg").InnerHtml.ShouldContain("M9.09 9a3 3 0 0 1 5.83 1");
    }
}
