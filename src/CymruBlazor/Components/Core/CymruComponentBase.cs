using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Core;

/// <summary>
/// Provides the common foundation for all CymruBlazor components.
///
/// This base class intentionally remains lightweight and supplies only
/// functionality that is universally applicable across the component library.
/// More specialised behaviour (interactive controls, forms, JavaScript interop,
/// validation, etc.) is provided by derived base classes.
/// </summary>
public abstract class CymruComponentBase : ComponentBase
{
    private string _id = string.Empty;
    private string _cssClass = string.Empty;
    private string _cssStyle = string.Empty;

    /// <summary>
    /// Gets the component identifier generator.
    /// </summary>
    [Inject]
    protected IComponentIdGenerator ComponentIdGenerator { get; set; } = default!;

    /// <summary>
    /// Gets or sets additional HTML attributes that do not correspond
    /// to strongly typed component parameters.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute.
    /// If no value is supplied, a deterministic unique identifier is generated.
    /// </summary>
    [Parameter]
    public string Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets inline CSS styles.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets the component's root CSS class.
    /// Derived components override this to provide their semantic base class.
    /// </summary>
    protected virtual string BaseCssClass => string.Empty;

    /// <summary>
    /// Gets the computed CSS class string.
    /// </summary>
    protected string CssClass => _cssClass;

    /// <summary>
    /// Gets the computed inline style string.
    /// </summary>
    protected string CssStyle => _cssStyle;

    /// <summary>
    /// Builds the component CSS class string.
    /// Derived components should override to append component-specific classes.
    /// </summary>
    protected virtual string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();

    /// <summary>
    /// Builds the component inline style string.
    /// Derived components should override to append component-specific styles.
    /// </summary>
    protected virtual string BuildCssStyle() =>
        StyleBuilder.Empty
            .AddStyle(Style)
            .Build();

    /// <summary>
    /// Determines whether the component should render.
    /// Derived components may override for performance optimisation.
    /// </summary>
    protected override bool ShouldRender() => true;

    /// <summary>
    /// Validates component parameters after they have been assigned.
    /// Derived components should override to enforce invariants.
    /// </summary>
    protected virtual void ValidateParameters() { }

    /// <inheritdoc />
    protected sealed override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(_id))
        {
            _id = ComponentIdGenerator.Create();
        }

        _cssClass = BuildCssClass();
        _cssStyle = BuildCssStyle();

        ValidateParameters();

        OnParametersValidated();
    }

    /// <summary>
    /// Called after parameter validation has completed successfully.
    /// </summary>
    protected virtual void OnParametersValidated() { }

    /// <summary>
    /// Attempts to retrieve an unmatched HTML attribute.
    /// </summary>
    /// <param name="attributeName">
    /// The attribute name.
    /// </param>
    /// <param name="value">
    /// The attribute value if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the attribute exists; otherwise <see langword="false"/>.
    /// </returns>
    protected bool TryGetAdditionalAttribute(
        string attributeName,
        [NotNullWhen(true)] out object? value)
    {
        value = null;

        return AdditionalAttributes is not null &&
               AdditionalAttributes.TryGetValue(attributeName, out value);
    }
}
