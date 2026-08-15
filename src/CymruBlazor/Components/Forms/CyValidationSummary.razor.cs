using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Layout;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A restyled wrapper around the framework's own
/// <see cref="Microsoft.AspNetCore.Components.Forms.ValidationSummary"/>,
/// following the NHS Wales/gov.uk-style "error summary" convention (a
/// titled box listing every validation failure). The framework component
/// already correctly aggregates <c>EditContext</c> validation messages -
/// the value added here is presentation, not new validation logic. Must
/// be used inside an <c>EditForm</c>/cascaded <c>EditContext</c>, same as
/// the framework component it wraps.
/// </summary>
public partial class CyValidationSummary : CyLayoutComponentBase
{
    /// <summary>
    /// Optional heading shown above the list of validation messages.
    /// Defaults to "There is a problem" (the standard gov.uk/NHS error
    /// summary heading). Pass <see langword="null"/> or an empty string
    /// to omit the heading entirely.
    /// </summary>
    [Parameter]
    public string? Title { get; set; } = "There is a problem";

    protected override string BaseCssClass => "cy-validation-summary";
}
