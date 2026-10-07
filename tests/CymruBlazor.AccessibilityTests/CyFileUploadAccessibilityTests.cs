using Bunit;
using CymruBlazor.Components.Forms;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests;

public sealed class CyFileUploadAccessibilityTests : AxeTestBase
{
    private string Markup(Action<ComponentParameterCollectionBuilder<CyFileUpload>>? configure = null) =>
        Render<CyFileUpload>(p =>
        {
            p.Add(c => c.Label, "Referral letter");
            configure?.Invoke(p);
        }).Markup;

    private string AllStates() => string.Join(
        Environment.NewLine,
        Markup(),
        Markup(p => p.Add(c => c.HintText, "A scan or photo").Add(c => c.Accept, ".pdf,image/*").Add(c => c.MaxFileSize, 2 * 1024 * 1024)),
        Markup(p => p.Add(c => c.Required, true).Add(c => c.Error, "Choose a file")),
        Markup(p => p.Add(c => c.Multiple, true).Add(c => c.MaxFiles, 3)),
        Markup(p => p.Add(c => c.Disabled, true)));

    [Fact]
    public async Task Should_Have_No_Violations_In_All_States()
    {
        var result = await ScanMarkupAsync(AllStates());

        result.Violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Should_Have_No_Violations_In_Dark_And_High_Contrast_Themes(string theme)
    {
        var result = await ScanMarkupAsync(AllStates(), theme);

        result.Violations.ShouldBeEmpty();
    }
}
