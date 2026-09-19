using System.Text;
using CymruBlazor.Components.Feedback;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyToastContainerAccessibilityTests : AxeTestBase
{
    private readonly ToastService _service = new();

    public CyToastContainerAccessibilityTests()
    {
        Services.AddSingleton<IToastService>(_service);
    }

    /// <summary>
    /// Renders one toast of every variant (sticky, so no timers run) and
    /// returns markup that also carries the container's own stylesheet.
    /// </summary>
    /// <remarks>
    /// CyToastContainer's CSS lives in a scoped <c>.razor.css</c>. In the
    /// real app Blazor rewrites its selectors to require a <c>b-xxxx</c>
    /// scope attribute; bUnit markup has no such attribute, so the bundled
    /// <c>CymruBlazor.styles.css</c> would not match anything and this scan
    /// would silently test an unstyled toast. The source stylesheet has no
    /// scoping in it, so it is inlined verbatim to make the scan real.
    /// </remarks>
    private string RenderAllVariantsWithStyles()
    {
        _service.Show("Changes saved.", ToastVariant.Success, TimeSpan.Zero);
        _service.Show("Heads up, this cannot be undone.", ToastVariant.Info, TimeSpan.Zero);
        _service.Show("Your session expires soon.", ToastVariant.Warning, TimeSpan.Zero);
        _service.Show("Could not save your changes.", ToastVariant.Danger, TimeSpan.Zero);

        var cut = Render<CyToastContainer>();

        return new StringBuilder()
            .Append("<style>")
            .Append(File.ReadAllText(FindToastStylesheet()))
            .AppendLine("</style>")
            .Append(cut.Markup)
            .ToString();
    }

    [Fact]
    public async Task Should_Have_No_Violations_With_All_Variants_In_Light_Theme()
    {
        var result = await ScanMarkupAsync(RenderAllVariantsWithStyles());

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var result = await ScanMarkupAsync(RenderAllVariantsWithStyles(), theme);

        result.Violations.ShouldBeEmpty();
    }

    private static string FindToastStylesheet()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "CymruBlazor.slnx")))
        {
            dir = dir.Parent;
        }

        var root = dir?.FullName
            ?? throw new InvalidOperationException(
                $"Could not locate the repository root (CymruBlazor.slnx) above '{AppContext.BaseDirectory}'.");

        return Path.Combine(root, "src", "CymruBlazor", "Components", "Feedback", "CyToastContainer.razor.css");
    }
}
