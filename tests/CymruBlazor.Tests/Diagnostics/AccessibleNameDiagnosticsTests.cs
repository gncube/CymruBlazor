using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Forms;
using CymruBlazor.Diagnostics;

namespace CymruBlazor.Tests.Diagnostics;

/// <summary>CY0005: a form field with no accessible name warns, in both modes, and never throws.</summary>
public sealed class AccessibleNameDiagnosticsTests : TestContextBase
{
    private sealed class Recorder(CyDiagnosticsMode mode) : ICyDiagnostics
    {
        public List<(string Code, string Message)> Warnings { get; } = [];

        public CyDiagnosticsMode Mode { get; } = mode;

        public void Warn(string code, string message) => Warnings.Add((code, message));
    }

    private Recorder Register(CyDiagnosticsMode mode = CyDiagnosticsMode.Lenient)
    {
        var recorder = new Recorder(mode);
        Services.AddSingleton<ICyDiagnostics>(recorder);
        return recorder;
    }

    private IRenderedComponent<CyTextBox> RenderTextBox(
        string? label,
        IReadOnlyDictionary<string, object>? attributes = null)
    {
        var text = string.Empty;

        return Render<CyTextBox>(p =>
        {
            if (label is not null)
            {
                p.Add(c => c.Label, label);
            }

            if (attributes is not null)
            {
                p.AddUnmatched("aria-label", attributes.GetValueOrDefault("aria-label")?.ToString() ?? string.Empty);
            }

            p.Add(c => c.Value, text)
             .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => text = v))
             .Add(c => c.ValueExpression, () => text);
        });
    }

    [Fact]
    public void A_Field_With_No_Label_Warns_Once_With_CY0005()
    {
        var recorder = Register();

        RenderTextBox(label: null);

        var warning = recorder.Warnings.ShouldHaveSingleItem();
        warning.Code.ShouldBe("CY0005");
        warning.Message.ShouldContain("CyTextBox");
        warning.Message.ShouldContain("Label");
    }

    [Fact]
    public void A_Generic_Field_Is_Named_Without_The_Arity_Suffix()
    {
        var recorder = Register();
        var value = string.Empty;

        Render<CySelect<string>>(p => p
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => value = v))
            .Add(c => c.ValueExpression, () => value));

        recorder.Warnings.ShouldHaveSingleItem().Message.ShouldStartWith("CySelect has no accessible name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_Blank_Label_Counts_As_No_Label(string label)
    {
        var recorder = Register();

        RenderTextBox(label);

        recorder.Warnings.ShouldHaveSingleItem().Code.ShouldBe("CY0005");
    }

    [Fact]
    public void A_Field_With_A_Label_Does_Not_Warn()
    {
        var recorder = Register();

        RenderTextBox("Full name");

        recorder.Warnings.ShouldBeEmpty();
    }

    [Fact]
    public void An_AriaLabel_Counts_As_An_Accessible_Name()
    {
        var recorder = Register();

        RenderTextBox(label: null, new Dictionary<string, object> { ["aria-label"] = "Search" });

        recorder.Warnings.ShouldBeEmpty();
    }

    [Fact]
    public void A_Blank_AriaLabel_Does_Not_Count()
    {
        var recorder = Register();

        RenderTextBox(label: null, new Dictionary<string, object> { ["aria-label"] = " " });

        recorder.Warnings.ShouldHaveSingleItem().Code.ShouldBe("CY0005");
    }

    [Fact]
    public void It_Warns_Rather_Than_Throws_Even_In_Strict_Mode()
    {
        var recorder = Register(CyDiagnosticsMode.Strict);

        Should.NotThrow(() => RenderTextBox(label: null));

        recorder.Warnings.ShouldHaveSingleItem().Code.ShouldBe("CY0005");
    }

    [Fact]
    public void Without_A_Registered_Diagnostics_Service_Rendering_Still_Works()
    {
        var cut = RenderTextBox(label: null);

        cut.Find("input").ShouldNotBeNull();
    }
}
