namespace CymruBlazor.Components.Forms;

/// <summary>
/// What a <see cref="CyField"/> hands to the control it wraps, so the control
/// is correctly labelled, described and flagged without the author wiring ids
/// by hand.
/// </summary>
public sealed class CyFieldContext
{
    internal CyFieldContext(
        string inputId,
        string? hintId,
        string? errorId,
        string? errorText,
        bool required)
    {
        InputId = inputId;
        HintId = hintId;
        ErrorId = errorId;
        ErrorText = errorText;
        Required = required;
    }

    /// <summary>The id the control must carry; the field's label points at it.</summary>
    public string InputId { get; }

    /// <summary>The id of the hint paragraph, or <see langword="null"/> when there is no hint.</summary>
    public string? HintId { get; }

    /// <summary>The id of the error paragraph, or <see langword="null"/> when there is no error.</summary>
    public string? ErrorId { get; }

    /// <summary>The error being shown, or <see langword="null"/>.</summary>
    public string? ErrorText { get; }

    /// <summary>Whether the field is marked required.</summary>
    public bool Required { get; }

    /// <summary>Whether an error is currently shown.</summary>
    public bool Invalid => ErrorText is not null;

    /// <summary>
    /// The ids the control should be described by (hint and/or error), or
    /// <see langword="null"/> when there are none.
    /// </summary>
    public string? DescribedBy => (HintId, ErrorId) switch
    {
        (null, null) => null,
        (not null, null) => HintId,
        (null, not null) => ErrorId,
        _ => $"{HintId} {ErrorId}"
    };

    /// <summary>
    /// The attributes to splat onto the control: <c>id</c>,
    /// <c>aria-describedby</c> (only when there is something to describe it),
    /// and <c>aria-invalid</c>/<c>aria-required</c> (only when true), e.g.
    /// <c>&lt;input @attributes="field.Attributes" class="cy-input" /&gt;</c>.
    /// </summary>
    public IReadOnlyDictionary<string, object> Attributes
    {
        get
        {
            var attributes = new Dictionary<string, object> { ["id"] = InputId };

            if (DescribedBy is { } describedBy)
            {
                attributes["aria-describedby"] = describedBy;
            }

            if (Invalid)
            {
                attributes["aria-invalid"] = "true";
            }

            if (Required)
            {
                attributes["aria-required"] = "true";
            }

            return attributes;
        }
    }
}
