using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Branding;
using CymruBlazor.Components.Button;
using CymruBlazor.Components.Content;
using CymruBlazor.Components.Forms;
using CymruBlazor.Contracts;
using CymruBlazor.Enums;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Components;

/// <summary>
/// Roadmap D6: the shared contracts are implemented where the property already existed, which
/// makes cross-component checks possible (one theory over every <see cref="IHasSize"/>), and the
/// two contracts nothing implements are marked obsolete for removal in 2.0.0.
/// </summary>
public sealed class ComponentContractTests : TestContextBase
{
    [Theory]
    [InlineData(typeof(CyButton))]
    [InlineData(typeof(CyBrandLogo))]
    [InlineData(typeof(CyDialog))]
    public void Components_With_A_Size_Implement_IHasSize(Type component)
    {
        typeof(IHasSize).IsAssignableFrom(component).ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(CyButton))]
    [InlineData(typeof(CyBadge))]
    [InlineData(typeof(CyAlert))]
    public void Components_With_A_Semantic_Colour_Implement_IHasColour(Type component)
    {
        typeof(IHasColour).IsAssignableFrom(component).ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(CyButton))]
    [InlineData(typeof(CyTextBox))]
    [InlineData(typeof(CyLanguageToggle))]
    public void Components_That_Can_Be_Disabled_Implement_IHasDisabledState(Type component)
    {
        typeof(IHasDisabledState).IsAssignableFrom(component).ShouldBeTrue();
    }

    [Fact]
    public void Form_Fields_Implement_IHasValidationState()
    {
        typeof(IHasValidationState).IsAssignableFrom(typeof(CyTextBox)).ShouldBeTrue();
    }

    [Fact]
    public void IHasSize_Reports_The_Size_Parameter()
    {
        var button = Render<CyButton>(p => p.Add(b => b.Size, ComponentSize.Large));

        ((IHasSize)button.Instance).Size.ShouldBe(ComponentSize.Large);
    }

    [Fact]
    public void IHasColour_Reports_Variant_Or_Severity()
    {
        var button = Render<CyButton>(p => p.Add(b => b.Variant, ComponentColour.Danger));
        var alert = Render<CyAlert>(p => p.Add(a => a.Severity, ComponentColour.Warning));
        var badge = Render<CyBadge>(p => p.Add(b => b.Variant, ComponentColour.Success));

        ((IHasColour)button.Instance).Colour.ShouldBe(ComponentColour.Danger);
        ((IHasColour)alert.Instance).Colour.ShouldBe(ComponentColour.Warning);
        ((IHasColour)badge.Instance).Colour.ShouldBe(ComponentColour.Success);
    }

    [Fact]
    public void IHasDisabledState_Reports_The_Disabled_Parameter()
    {
        var button = Render<CyButton>(p => p.Add(b => b.Disabled, true));

        ((IHasDisabledState)button.Instance).Disabled.ShouldBeTrue();
    }

    [Fact]
    public void IHasValidationState_Is_Invalid_Only_While_The_Field_Has_A_Validation_Message()
    {
        var model = new TextModel();
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);

        var cut = Render<CascadingValue<EditContext>>(p => p
            .Add(c => c.Value, editContext)
            .AddChildContent<CyTextBox>(t => t
                .Add(x => x.Value, model.Name)
                .Add(x => x.ValueExpression, () => model.Name)
                .Add(x => x.Label, "Name")));

        var field = cut.FindComponent<CyTextBox>().Instance;

        ((IHasValidationState)field).ValidationState.ShouldBe(ValidationState.Unspecified);

        messages.Add(editContext.Field(nameof(TextModel.Name)), "Required");
        editContext.NotifyValidationStateChanged();

        ((IHasValidationState)field).ValidationState.ShouldBe(ValidationState.Invalid);
    }

    [Theory]
    [InlineData("CymruBlazor.Contracts.IHasVariant")]
    [InlineData("CymruBlazor.Contracts.IHasIcon")]
    [InlineData("CymruBlazor.Enums.ComponentVariant")]
    [InlineData("CymruBlazor.Enums.IconPosition")]
    public void Unused_Contracts_Are_Marked_Obsolete_For_Removal_In_2_0(string typeName)
    {
        var type = typeof(IHasSize).Assembly.GetType(typeName, throwOnError: true)!;

        var obsolete = (ObsoleteAttribute?)Attribute.GetCustomAttribute(type, typeof(ObsoleteAttribute));

        obsolete.ShouldNotBeNull();
        obsolete.Message.ShouldNotBeNull();
        obsolete.Message!.ShouldContain("2.0.0");
    }

    [Fact]
    public void AddCymruBlazor_Registers_The_Real_Focus_Manager()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCymruBlazor();

        var descriptor = services.Last(d => d.ServiceType == typeof(IFocusManager));

        descriptor.ImplementationType.ShouldBe(typeof(JsFocusManager));
    }

    private sealed class TextModel
    {
        public string Name { get; set; } = string.Empty;
    }
}
