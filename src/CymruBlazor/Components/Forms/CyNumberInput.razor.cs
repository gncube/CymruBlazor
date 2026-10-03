using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// A numeric input field for <c>int</c>, <c>long</c>, <c>short</c>,
/// <c>float</c>, <c>double</c> and <c>decimal</c> (and their nullable forms),
/// with a <see cref="Unit"/> suffix and <see cref="Min"/>/<see cref="Max"/>/
/// <see cref="Step"/> validation. Works with or without an
/// <c>&lt;EditForm&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// Like <see cref="CyTextBox"/>, this renders <c>type="text"</c> with an
/// <c>inputmode</c> hint rather than <c>type="number"</c>: a native number
/// input's spin buttons, locale-dependent separator and habit of silently
/// discarding what it cannot parse make it a poor fit for clinical data (see
/// the remarks on <see cref="CyTextBox"/>). Parsing, range and step checks are
/// done in C# against <see cref="Culture"/>, so the rules are the same on every
/// browser and the messages are yours to word.
/// </para>
/// <para>
/// A non-nullable <typeparamref name="TValue"/> cannot represent "empty", so an
/// empty box is reported as invalid; bind a nullable type (<c>int?</c>) for an
/// optional number.
/// </para>
/// </remarks>
public partial class CyNumberInput<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>
    : CyFormFieldComponentBase<TValue>
{
    /// <summary>Placeholder text. Never a substitute for <see cref="CyFormFieldComponentBase{TValue}.Label"/>.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// A short unit shown after the box (e.g. <c>"kg"</c>, <c>"mmol/L"</c>). It is
    /// exposed to assistive technology as part of the field's description, so a
    /// screen-reader user hears the unit with the field.
    /// </summary>
    [Parameter]
    public string? Unit { get; set; }

    /// <summary>The smallest accepted value, or <see langword="null"/> for no lower bound.</summary>
    [Parameter]
    public decimal? Min { get; set; }

    /// <summary>The largest accepted value, or <see langword="null"/> for no upper bound.</summary>
    [Parameter]
    public decimal? Max { get; set; }

    /// <summary>
    /// When set, only values that are a whole number of steps above
    /// <see cref="Min"/> (or above zero when <see cref="Min"/> is not set) are
    /// accepted. Must be greater than zero.
    /// </summary>
    [Parameter]
    public decimal? Step { get; set; }

    /// <summary>
    /// The culture used to read and show the number (decimal separator, etc.).
    /// Defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Overrides the <c>inputmode</c> hint. By default it is <c>"numeric"</c> for
    /// whole-number types and <c>"decimal"</c> for the others, and is left off
    /// when <see cref="Min"/> is negative (the on-screen numeric keypads on some
    /// platforms have no minus key). Set <c>"text"</c> to always get the full keyboard.
    /// </summary>
    [Parameter]
    public string? InputMode { get; set; }

    /// <summary>Message when the text is not a number.</summary>
    [Parameter]
    public string InvalidNumberMessage { get; set; } = "Enter a number";

    /// <summary>Message when the value is below <see cref="Min"/>. <c>{0}</c> is the minimum.</summary>
    [Parameter]
    public string BelowMinFormat { get; set; } = "Enter a number that is {0} or more";

    /// <summary>Message when the value is above <see cref="Max"/>. <c>{0}</c> is the maximum.</summary>
    [Parameter]
    public string AboveMaxFormat { get; set; } = "Enter a number that is {0} or less";

    /// <summary>Message when the value is not on a <see cref="Step"/>. <c>{0}</c> is the step.</summary>
    [Parameter]
    public string StepFormat { get; set; } = "Enter a number in steps of {0}";

    private static readonly Type Underlying = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

    private static bool IsIntegral => Underlying == typeof(int) || Underlying == typeof(long) || Underlying == typeof(short);

    private static bool IsSupported =>
        IsIntegral || Underlying == typeof(float) || Underlying == typeof(double) || Underlying == typeof(decimal);

    private CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentCulture;

    private string UnitId => $"{FieldId}-unit";

    private string? ResolvedInputMode
    {
        get
        {
            if (InputMode is not null)
            {
                return InputMode;
            }

            if (Min is < 0)
            {
                return null;
            }

            return IsIntegral ? "numeric" : "decimal";
        }
    }

    /// <summary>
    /// The base description ids (hint, error) plus the unit's id, unit first so
    /// the unit is read straight after the field name.
    /// </summary>
    private string? DescribedBy
    {
        get
        {
            var hasUnit = !string.IsNullOrWhiteSpace(Unit);

            if (!hasUnit)
            {
                return ComputedAriaDescribedBy;
            }

            return ComputedAriaDescribedBy is { } rest ? $"{UnitId} {rest}" : UnitId;
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!IsSupported)
        {
            throw new InvalidOperationException(
                $"{nameof(CyNumberInput<int>)} does not support '{typeof(TValue)}'. " +
                "Use int, long, short, float, double or decimal (or their nullable forms).");
        }

        if (Min is { } min && Max is { } max && min > max)
        {
            throw new InvalidOperationException(
                $"{nameof(CyNumberInput<int>)}.{nameof(Min)} ({min}) must not be greater than {nameof(Max)} ({max}).");
        }

        if (Step is <= 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyNumberInput<int>)}.{nameof(Step)} must be greater than zero.");
        }
    }

    /// <inheritdoc />
    protected override string? FormatValueAsString(TValue? value) =>
        value is null ? string.Empty : BindConverter.FormatValue(value, EffectiveCulture)?.ToString();

    /// <inheritdoc />
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        var culture = EffectiveCulture;

        if (!BindConverter.TryConvertTo<TValue>(value?.Trim(), culture, out var parsed))
        {
            return Fail(InvalidNumberMessage, out result, out validationErrorMessage);
        }

        if (parsed is not null)
        {
            if (!TryAsDecimal(parsed, out var number))
            {
                return Fail(InvalidNumberMessage, out result, out validationErrorMessage);
            }

            if (Min is { } min && number < min)
            {
                return Fail(string.Format(culture, BelowMinFormat, min), out result, out validationErrorMessage);
            }

            if (Max is { } max && number > max)
            {
                return Fail(string.Format(culture, AboveMaxFormat, max), out result, out validationErrorMessage);
            }

            if (Step is { } step)
            {
                var steps = (number - (Min ?? 0m)) / step;

                if (steps != decimal.Truncate(steps))
                {
                    return Fail(string.Format(culture, StepFormat, step), out result, out validationErrorMessage);
                }
            }
        }

        // The framework only keeps parse messages when there is an EditContext;
        // clear any standalone error left over from a previous bad entry.
        LocalError = null;
        result = parsed!;
        validationErrorMessage = null;
        return true;
    }

    private bool Fail(string message, out TValue? result, out string? validationErrorMessage)
    {
        // Inside an EditForm the framework stores the message on the
        // EditContext; with none, keep it here so the user still sees it.
        LocalError = EditContext is null ? message : null;
        result = default;
        validationErrorMessage = message;
        return false;
    }

    private static bool TryAsDecimal(object parsed, out decimal number)
    {
        try
        {
            number = Convert.ToDecimal(parsed, CultureInfo.InvariantCulture);
            return true;
        }
        catch (OverflowException)
        {
            // NaN, infinity, or a double beyond decimal's range.
            number = default;
            return false;
        }
    }
}
