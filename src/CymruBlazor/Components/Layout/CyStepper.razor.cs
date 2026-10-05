using System.Globalization;
using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// A progress indicator for a multi-step journey - an order wizard, a "check your answers" flow or a runtime
/// preview - laid out horizontally or vertically.
/// </summary>
/// <remarks>
/// <para>
/// Rendered as a <c>nav</c> with an ordered list. The current step carries <c>aria-current="step"</c>; the state of
/// every other step is spoken as visually hidden text ("completed", "not started", "has an error") and is also shown
/// by an icon or number, never by colour alone.
/// </para>
/// <para>
/// With <see cref="Clickable"/> set, completed steps and the current step become buttons (or links when the
/// <see cref="CyStepItem.Href"/> is set). Upcoming steps stay plain text unless <see cref="AllowForwardNavigation"/> is
/// true, so users cannot skip steps that depend on earlier answers. Below 40rem the horizontal layout collapses to a
/// one-line "Step 2 of 5: Title" summary.
/// </para>
/// <para>
/// <see cref="Current"/> is zero-based and two-way bindable (<c>@bind-Current</c>). Selecting a step sets it, raises
/// <see cref="CurrentChanged"/> and then <see cref="OnStepSelected"/>.
/// </para>
/// </remarks>
public partial class CyStepper : CyComponentBase
{
    private static readonly CyStepperText s_defaultText = new();

    /// <summary>The accessible name of the navigation landmark, for example "Order progress". Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>The steps, in order.</summary>
    [Parameter]
    public IReadOnlyList<CyStepItem> Steps { get; set; } = [];

    /// <summary>Zero-based index of the current step. Values outside the list leave every step complete or upcoming.</summary>
    [Parameter]
    public int Current { get; set; }

    /// <summary>Raised when the user selects a step. Enables <c>@bind-Current</c>.</summary>
    [Parameter]
    public EventCallback<int> CurrentChanged { get; set; }

    /// <summary>Raised after <see cref="CurrentChanged"/> with the zero-based index of the step the user selected.</summary>
    [Parameter]
    public EventCallback<int> OnStepSelected { get; set; }

    /// <summary>Whether completed and current steps can be selected. Defaults to false.</summary>
    [Parameter]
    public bool Clickable { get; set; }

    /// <summary>With <see cref="Clickable"/>, also lets users select steps they have not reached. Defaults to false.</summary>
    [Parameter]
    public bool AllowForwardNavigation { get; set; }

    /// <summary>Horizontal (the default) or vertical.</summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>The phrases the stepper shows. Unset properties keep their English defaults.</summary>
    [Parameter]
    public CyStepperText? Text { get; set; }

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-stepper";

    private CyStepperText Strings => Text ?? s_defaultText;

    private string? SummaryText =>
        Orientation == Orientation.Horizontal && Current >= 0 && Current < Steps.Count
            ? string.Format(CultureInfo.CurrentCulture, Strings.StepOfFormat, Current + 1, Steps.Count, Steps[Current].Title)
            : null;

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Orientation == Orientation.Vertical ? "cy-stepper--vertical" : "cy-stepper--horizontal")
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyStepper)}.{nameof(Label)} must not be empty: it names the navigation landmark.");
        }

        ArgumentNullException.ThrowIfNull(Steps);

        for (var i = 0; i < Steps.Count; i++)
        {
            if (Steps[i] is null || string.IsNullOrWhiteSpace(Steps[i].Title))
            {
                throw new InvalidOperationException(
                    $"{nameof(CyStepper)}.{nameof(Steps)}[{i}] needs a title: it names the step.");
            }
        }
    }

    private CyStepStatus StatusOf(int index)
    {
        var declared = Steps[index].Status;

        if (declared != CyStepStatus.Auto)
        {
            return declared;
        }

        if (index < Current)
        {
            return CyStepStatus.Complete;
        }

        return index == Current ? CyStepStatus.Current : CyStepStatus.Upcoming;
    }

    private bool IsCurrent(int index) => index == Current || Steps[index].Status == CyStepStatus.Current;

    private bool IsSelectable(int index)
    {
        if (!Clickable || Steps[index].Disabled)
        {
            return false;
        }

        var status = StatusOf(index);

        return AllowForwardNavigation || status != CyStepStatus.Upcoming;
    }

    private string? HiddenStatusText(CyStepStatus status) => status switch
    {
        CyStepStatus.Complete => Strings.Completed,
        CyStepStatus.Upcoming => Strings.NotStarted,
        CyStepStatus.Error => Strings.HasError,
        _ => null
    };

    private static string ItemClass(CyStepStatus status, bool isCurrent) =>
        CssBuilder.Empty
            .AddClass("cy-stepper__item")
            .AddClass($"cy-stepper__item--{status.ToString().ToLowerInvariant()}")
            .AddClass("cy-stepper__item--current", isCurrent && status != CyStepStatus.Current)
            .Build();

    private async Task SelectAsync(int index)
    {
        Current = index;

        if (CurrentChanged.HasDelegate)
        {
            await CurrentChanged.InvokeAsync(index);
        }

        if (OnStepSelected.HasDelegate)
        {
            await OnStepSelected.InvokeAsync(index);
        }
    }
}
