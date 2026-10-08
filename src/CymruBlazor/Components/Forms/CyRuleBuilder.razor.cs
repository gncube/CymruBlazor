using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Accessibility;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// Builds a nested rule - "all of these conditions, or any of these" - from a list of fields, with keyboard- and
/// screen-reader-friendly controls. It edits a <see cref="CyRuleGroup"/> model; it does not evaluate or serialise it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Model.</b> Bind <see cref="Value"/> (a <see cref="CyRuleGroup"/>); every change produces a new root object, so it
/// works with change detection and undo. <see cref="Fields"/> says what can be tested. Evaluating the rule and storing it
/// are the application's job: the library offers <see cref="CyRuleSummary"/>, <see cref="CyRuleValidator"/> and the
/// pure edit functions in <see cref="CyRuleEdits"/>, and sends nothing anywhere.
/// </para>
/// <para>
/// <b>Accessibility.</b> Each group is a <c>fieldset</c> whose legend says what it means ("All of the following"); each
/// condition is a named group ("Condition 2 of 3"). Every action is a real button - there is no drag-and-drop, so
/// nothing needs a pointer (WCAG 2.5.7). After adding, removing or moving, focus goes somewhere sensible and one polite
/// status message says what happened. Removing a group that holds conditions asks first when an
/// <see cref="ICyConfirmService"/> is available.
/// </para>
/// <para>
/// <b>Validation.</b> Incomplete conditions are listed in <see cref="IsValid"/> and reported through
/// <see cref="OnValidationChanged"/> all the time, but their messages are shown only when <see cref="ShowIssues"/> is
/// true or after <see cref="ValidateAsync"/> has been called (for example when the user presses Save), so people are
/// not told off mid-edit. Checks here are advisory: validate again wherever the rule is used.
/// </para>
/// <para>
/// <b>Limits.</b> <see cref="Limits"/> caps nesting (default 3 levels) and the number of conditions (default 50); the
/// controls that would break a limit are disabled and a hint says why. No script is needed.
/// </para>
/// </remarks>
public partial class CyRuleBuilder : CyInteractiveComponentBase
{
    private static readonly CyRuleBuilderText DefaultText = new();
    private static readonly CyRuleLimits DefaultLimits = new();
    private static readonly IReadOnlyList<CyOption<string>> NoOptions = [];

    private readonly CyRuleGroup _fallbackRoot = new();
    private readonly Dictionary<string, CyRuleField> _fieldByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<string>>>>> _providers =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, string>> _codedLabels = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _groupNumbers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CyRuleIssueCode> _issueById = new(StringComparer.Ordinal);

    private CyRuleGroup _root;
    private IReadOnlyList<CyRuleField> _fields = [];
    private IReadOnlyList<CyRuleField>? _lastFields;
    private List<CyOption<string>> _fieldOptions = [];
    private List<CyOption<CyRuleCombinator>> _combinators = [];
    private IReadOnlyList<CyRuleIssue> _issues = [];
    private bool _showIssues;
    private string _announcement = string.Empty;
    private bool _announceToggle;
    private string? _pendingFocus;

    // Targets for the inner editors' required ValueExpression. They are never read: the builder sits outside any
    // EditContext (see the cascading value in the markup) and takes values through the change callbacks.
    private string? UnusedText { get; set; }

    private bool UnusedFlag { get; set; }

    private decimal? UnusedNumber { get; set; }

    private DateOnly? UnusedDate { get; set; }

    /// <summary>Creates the component.</summary>
    public CyRuleBuilder()
    {
        _root = _fallbackRoot;
    }

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    /// <summary>The accessible name of the whole builder, for example "Eligibility rule". Required.</summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>The rule being edited. Two-way bindable; every change is a new root object. When null the builder starts empty.</summary>
    [Parameter]
    public CyRuleGroup? Value { get; set; }

    /// <summary>Raised with the new root after every change.</summary>
    [Parameter]
    public EventCallback<CyRuleGroup> ValueChanged { get; set; }

    /// <summary>What a condition can test. Keys must be unique.</summary>
    [Parameter]
    public IReadOnlyList<CyRuleField>? Fields { get; set; }

    /// <summary>Nesting, size and minimum limits. Defaults to 3 levels, 50 conditions and no minimum.</summary>
    [Parameter]
    public CyRuleLimits? Limits { get; set; }

    /// <summary>Shows the rule as text, with no controls.</summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>Shows a plain-language summary of the rule under the builder (default true). It is not a live region.</summary>
    [Parameter]
    public bool ShowSummary { get; set; } = true;

    /// <summary>Shows the message of every incomplete condition. See the class remarks for when to set it.</summary>
    [Parameter]
    public bool ShowIssues { get; set; }

    /// <summary>Raised when the set of incomplete conditions changes, with the current issues (empty when the rule is complete).</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<CyRuleIssue>> OnValidationChanged { get; set; }

    /// <summary>Replaces the English phrases. Unset properties keep their defaults.</summary>
    [Parameter]
    public CyRuleBuilderText? Text { get; set; }

    /// <summary>True when every condition is complete.</summary>
    public bool IsValid => _issues.Count == 0;

    /// <summary>The incomplete conditions, in document order.</summary>
    public IReadOnlyList<CyRuleIssue> Issues => _issues;

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-rule";

    private CyRuleBuilderText Strings => Text ?? DefaultText;

    private CyRuleLimits EffectiveLimits => Limits ?? DefaultLimits;

    private bool Editable => !ReadOnly && !Disabled;

    private bool IssuesVisible => ShowIssues || _showIssues;

    private string Summary => CyRuleSummary.Describe(_root, _fields, Strings);

    private string MinHintId => $"{Id}-min-hint";

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyRuleBuilder)}.{nameof(Label)} must not be empty: it names the builder.");
        }

        var limits = EffectiveLimits;

        if (limits.MaxDepth < 1 || limits.MaxConditions < 1 || limits.MinConditions < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(CyRuleBuilder)}.{nameof(Limits)}: MaxDepth and MaxConditions must be at least 1 and MinConditions at least 0.");
        }
    }

    /// <inheritdoc />
    protected override void OnParametersValidated()
    {
        if (!ReferenceEquals(_lastFields, Fields))
        {
            _lastFields = Fields;
            _fields = Fields ?? [];
            IndexFields();
        }

        _root = Value ?? _fallbackRoot;
        _combinators =
        [
            new CyOption<CyRuleCombinator>(CyRuleCombinator.And, Strings.MatchAll),
            new CyOption<CyRuleCombinator>(CyRuleCombinator.Or, Strings.MatchAny)
        ];

        Refresh();
    }

    /// <summary>
    /// Shows every incomplete condition's message and moves focus to the first one. Call it when the user tries to save.
    /// </summary>
    /// <returns><see langword="true"/> when the rule is complete.</returns>
    public Task<bool> ValidateAsync()
    {
        _showIssues = true;

        if (_issues.Count > 0)
        {
            var first = CyRuleEdits.Find(_root, _issues[0].NodeId);
            _pendingFocus = first is null ? null : FirstControlId(first);
        }

        StateHasChanged();
        return Task.FromResult(_issues.Count == 0);
    }

    private void IndexFields()
    {
        _fieldByKey.Clear();
        _providers.Clear();
        _codedLabels.Clear();

        foreach (var field in _fields)
        {
            if (!_fieldByKey.TryAdd(field.Key, field))
            {
                throw new InvalidOperationException(
                    $"{nameof(CyRuleBuilder)}.{nameof(Fields)} contains the key '{field.Key}' more than once. Keys must be unique.");
            }
        }

        _fieldOptions = [.. _fields.Select(f => new CyOption<string>(f.Key, f.Label))];
    }

    private CyRuleField? Lookup(string? key) =>
        key is not null && _fieldByKey.TryGetValue(key, out var field) ? field : null;

    // ------------------------------------------------------------------ derived state

    private bool Refresh()
    {
        _groupNumbers.Clear();
        var number = 0;
        NumberGroups(_root, ref number);

        var issues = CyRuleValidator.Validate(_root, _fields);
        var changed = !issues.SequenceEqual(_issues);
        _issues = issues;

        _issueById.Clear();
        foreach (var issue in issues)
        {
            _issueById[issue.NodeId] = issue.Code;
        }

        return changed;
    }

    private void NumberGroups(CyRuleGroup group, ref int number)
    {
        _groupNumbers[group.Id] = number;

        foreach (var child in group.Children)
        {
            if (child is CyRuleGroup nested)
            {
                number++;
                NumberGroups(nested, ref number);
            }
        }
    }

    private static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    private string Eid(CyRuleNode node, string part) => $"{Id}-{node.Id}-{part}";

    private string Legend(CyRuleGroup group) => CyRuleSummary.GroupLegend(group, Strings);

    private string TargetName(CyRuleGroup group) =>
        _groupNumbers.TryGetValue(group.Id, out var number) && number > 0
            ? Format(Strings.GroupName, number)
            : Strings.TopLevel;

    private static string GroupClass(bool root) => root ? "cy-rule__group cy-rule__group--root" : "cy-rule__group";

    private string? IssueIdOf(CyRuleCondition condition) =>
        IssuesVisible && _issueById.ContainsKey(condition.Id) ? Eid(condition, "issue") : null;

    private string? IssueTextOf(CyRuleCondition condition)
    {
        if (!IssuesVisible || !_issueById.TryGetValue(condition.Id, out var code))
        {
            return null;
        }

        return code switch
        {
            CyRuleIssueCode.NoField => Strings.IssueNoField,
            CyRuleIssueCode.UnknownField => Strings.IssueUnknownField,
            CyRuleIssueCode.NoOperator => Strings.IssueNoOperator,
            CyRuleIssueCode.UnknownOperator => Strings.IssueUnknownOperator,
            CyRuleIssueCode.InvalidRange => Strings.IssueInvalidRange,
            _ => Strings.IssueMissingValue
        };
    }

    private IReadOnlyList<CyOption<string>> OperatorOptions(CyRuleField? field)
    {
        if (field is null)
        {
            return NoOptions;
        }

        return [.. CyRuleOperators.Resolve(field).Select(key => new CyOption<string>(key, CyRuleOperators.Label(key, Strings)))];
    }

    private static string? TextOf(object? value) => value as string;

    private static decimal? NumberOf(object? value) =>
        CyRuleValues.TryNumber(value, out var number) ? number : null;

    private static DateOnly? DateOf(object? value) =>
        CyRuleValues.TryDate(value, out var date) ? date : null;

    private static string? LabelOf(CyRuleCondition condition, bool second) => second ? condition.Value2Label : condition.ValueLabel;

    // ------------------------------------------------------------------ limits

    private bool AtMaxConditions => CyRuleEdits.CountConditions(_root) >= EffectiveLimits.MaxConditions;

    private bool AtMaxDepth(int depth) => depth + 1 > EffectiveLimits.MaxDepth;

    private bool AtMinConditions =>
        EffectiveLimits.MinConditions > 0 && CyRuleEdits.CountConditions(_root) <= EffectiveLimits.MinConditions;

    private bool CanRemove(CyRuleNode node) =>
        !ReferenceEquals(CyRuleEdits.Remove(_root, node.Id, EffectiveLimits), _root);

    private bool CanDuplicate(CyRuleNode node) =>
        !ReferenceEquals(CyRuleEdits.Duplicate(_root, node.Id, EffectiveLimits), _root);

    private bool CanWrap(CyRuleNode node) =>
        !ReferenceEquals(CyRuleEdits.Wrap(_root, node.Id, EffectiveLimits), _root);

    private List<CyRuleGroup> MoveTargets(CyRuleNode node)
    {
        var targets = new List<CyRuleGroup>();
        var parent = CyRuleEdits.FindParent(_root, node.Id);
        CollectTargets(_root, node, parent, targets);
        return targets;
    }

    private void CollectTargets(CyRuleGroup group, CyRuleNode node, CyRuleGroup? parent, List<CyRuleGroup> targets)
    {
        if (!ReferenceEquals(group, parent) &&
            !string.Equals(group.Id, node.Id, StringComparison.Ordinal) &&
            !ReferenceEquals(CyRuleEdits.MoveTo(_root, node.Id, group.Id, int.MaxValue, EffectiveLimits), _root))
        {
            targets.Add(group);
        }

        foreach (var child in group.Children)
        {
            if (child is CyRuleGroup nested)
            {
                CollectTargets(nested, node, parent, targets);
            }
        }
    }

    // ------------------------------------------------------------------ focus targets

    /// <summary>The id of the first control inside a node: a condition's field picker, or the first one of a group.</summary>
    private string FirstControlId(CyRuleNode node) => node switch
    {
        CyRuleGroup group when group.Children.Count > 0 => FirstControlId(group.Children[0]),
        CyRuleGroup group => Eid(group, "add"),
        _ => Eid(node, "field")
    };

    // ------------------------------------------------------------------ committing edits

    private async Task ApplyAsync(CyRuleGroup next)
    {
        _root = next;
        Value = next;
        var issuesChanged = Refresh();

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(next);
        }

        if (issuesChanged && OnValidationChanged.HasDelegate)
        {
            await OnValidationChanged.InvokeAsync(_issues);
        }
    }

    private void Announce(string message)
    {
        // Two identical messages in a row are not announced twice, so alternate a trailing space.
        _announceToggle = !_announceToggle;
        _announcement = _announceToggle ? message : message + " ";
    }

    private async Task TryEditAsync(
        Func<CyRuleGroup, CyRuleGroup> edit,
        Func<CyRuleGroup, CyRuleGroup, string> announce,
        Func<CyRuleGroup, CyRuleGroup, string?> focus)
    {
        if (!Editable)
        {
            return;
        }

        var before = _root;
        var next = edit(before);

        if (ReferenceEquals(next, before))
        {
            Announce(Strings.ActionNotAllowed);
            return;
        }

        await ApplyAsync(next);
        Announce(announce(before, next));
        _pendingFocus = focus(before, next);
    }

    private async Task EditConditionAsync(string id, Func<CyRuleCondition, CyRuleCondition> change)
    {
        if (!Editable)
        {
            return;
        }

        var next = CyRuleEdits.Update(_root, id, node => node is CyRuleCondition condition ? change(condition) : node);

        if (!ReferenceEquals(next, _root))
        {
            await ApplyAsync(next);
        }
    }

    private async Task OnCombinatorAsync(string groupId, CyRuleCombinator combinator)
    {
        if (!Editable)
        {
            return;
        }

        var next = CyRuleEdits.SetCombinator(_root, groupId, combinator);

        if (!ReferenceEquals(next, _root))
        {
            await ApplyAsync(next);
        }
    }

    private async Task OnNotAsync(string groupId, bool not)
    {
        if (!Editable)
        {
            return;
        }

        var next = CyRuleEdits.SetNot(_root, groupId, not);

        if (!ReferenceEquals(next, _root))
        {
            await ApplyAsync(next);
        }
    }

    // ------------------------------------------------------------------ condition edits

    private async Task OnFieldChangedAsync(string id, string? key)
    {
        if (CyRuleEdits.Find(_root, id) is not CyRuleCondition current)
        {
            return;
        }

        var newKey = string.IsNullOrWhiteSpace(key) ? null : key;

        if (string.Equals(current.FieldKey, newKey, StringComparison.Ordinal))
        {
            return;
        }

        var oldField = Lookup(current.FieldKey);
        var newField = Lookup(newKey);

        var keepOperator = newField is not null && current.OperatorKey is not null &&
            CyRuleOperators.Resolve(newField).Contains(current.OperatorKey);

        var keepValue = keepOperator && oldField is not null && newField is not null && oldField.Type == newField.Type &&
            newField.Type is CyRuleFieldType.Text or CyRuleFieldType.Number or CyRuleFieldType.Date;

        var hasValue = !CyRuleValues.IsBlank(current.Value) || !CyRuleValues.IsBlank(current.Value2);
        var reset = (current.OperatorKey is not null && !keepOperator) || (hasValue && !keepValue);

        await EditConditionAsync(id, c => c with
        {
            FieldKey = newKey,
            OperatorKey = keepOperator ? c.OperatorKey : null,
            Value = keepValue ? c.Value : null,
            Value2 = keepValue ? c.Value2 : null,
            ValueLabel = null,
            Value2Label = null
        });

        if (reset)
        {
            Announce(Strings.FieldChanged);
        }
    }

    private Task OnOperatorChangedAsync(string id, string? key)
    {
        var newKey = string.IsNullOrWhiteSpace(key) ? null : key;
        var arity = newKey is null ? 0 : CyRuleOperators.Arity(newKey);

        return EditConditionAsync(id, c => c with
        {
            OperatorKey = newKey,
            Value = arity >= 1 ? c.Value : null,
            ValueLabel = arity >= 1 ? c.ValueLabel : null,
            Value2 = arity >= 2 ? c.Value2 : null,
            Value2Label = arity >= 2 ? c.Value2Label : null
        });
    }

    private Task SetValueAsync(string id, bool second, object? value, string? label = null)
    {
        var stored = value is string text && string.IsNullOrWhiteSpace(text) ? null : value;
        var storedLabel = stored is null ? null : label;

        return EditConditionAsync(id, c => second
            ? c with { Value2 = stored, Value2Label = storedLabel }
            : c with { Value = stored, ValueLabel = storedLabel });
    }

    private Task SetChoiceAsync(string id, CyRuleField field, bool second, string? key)
    {
        var label = key is null ? null : field.Options?.FirstOrDefault(o => string.Equals(o.Value, key, StringComparison.Ordinal))?.Text;
        return SetValueAsync(id, second, key, label ?? key);
    }

    private Task SetCodedAsync(string id, CyRuleField field, bool second, string? key)
    {
        string? label = null;

        if (!string.IsNullOrEmpty(key))
        {
            label = _codedLabels.TryGetValue(field.Key, out var labels) && labels.TryGetValue(key, out var known)
                ? known
                : field.Options?.FirstOrDefault(o => string.Equals(o.Value, key, StringComparison.Ordinal))?.Text ?? key;
        }

        return SetValueAsync(id, second, key, label);
    }

    private Func<CyComboboxRequest, CancellationToken, Task<IReadOnlyList<CyOption<string>>>>? ProviderFor(CyRuleField field)
    {
        var inner = field.ItemsProvider;

        if (inner is null)
        {
            return null;
        }

        if (_providers.TryGetValue(field.Key, out var existing))
        {
            return existing;
        }

        if (!_codedLabels.TryGetValue(field.Key, out var labels))
        {
            labels = new Dictionary<string, string>(StringComparer.Ordinal);
            _codedLabels[field.Key] = labels;
        }

        async Task<IReadOnlyList<CyOption<string>>> Wrapped(CyComboboxRequest request, CancellationToken cancellationToken)
        {
            var result = await inner(request, cancellationToken);

            foreach (var option in result)
            {
                labels[option.Value] = option.Text;
            }

            return result;
        }

        _providers[field.Key] = Wrapped;
        return Wrapped;
    }

    // ------------------------------------------------------------------ structure edits

    private Task AddConditionAsync(string groupId) =>
        TryEditAsync(
            root => CyRuleEdits.AddCondition(root, groupId, null, null, EffectiveLimits),
            (_, next) => Format(Strings.ConditionAdded, (CyRuleEdits.Find(next, groupId) as CyRuleGroup)?.Children.Count ?? 1),
            (_, next) => LastChildFocus(next, groupId));

    private Task AddGroupAsync(string groupId) =>
        TryEditAsync(
            root => CyRuleEdits.AddGroup(root, groupId, null, EffectiveLimits),
            (_, _) => Strings.GroupAdded,
            (_, next) => LastChildFocus(next, groupId));

    private string? LastChildFocus(CyRuleGroup root, string groupId) =>
        CyRuleEdits.Find(root, groupId) is CyRuleGroup { Children.Count: > 0 } group
            ? FirstControlId(group.Children[^1])
            : null;

    private async Task RemoveAsync(string id)
    {
        if (!Editable || CyRuleEdits.Find(_root, id) is not { } node)
        {
            return;
        }

        if (node is CyRuleGroup group && CyRuleEdits.CountConditions(group) > 0 && !await ConfirmRemoveGroupAsync(group))
        {
            return;
        }

        var isGroup = node is CyRuleGroup;

        await TryEditAsync(
            root => CyRuleEdits.Remove(root, id, EffectiveLimits),
            (_, _) => isGroup ? Strings.GroupRemoved : Strings.ConditionRemoved,
            (before, _) => FocusAfterRemoval(before, id));
    }

    private string? FocusAfterRemoval(CyRuleGroup before, string id)
    {
        var parent = CyRuleEdits.FindParent(before, id);

        if (parent is null)
        {
            return null;
        }

        var index = parent.Children.ToList().FindIndex(c => string.Equals(c.Id, id, StringComparison.Ordinal));

        if (index + 1 < parent.Children.Count)
        {
            return FirstControlId(parent.Children[index + 1]);
        }

        return index > 0 ? FirstControlId(parent.Children[index - 1]) : Eid(parent, "add");
    }

    private async Task<bool> ConfirmRemoveGroupAsync(CyRuleGroup group)
    {
        if (Services.GetService(typeof(ICyConfirmService)) is not ICyConfirmService confirm)
        {
            return true;
        }

        try
        {
            return await confirm.ConfirmAsync(
                new CyConfirmOptions
                {
                    Title = Strings.ConfirmRemoveGroupTitle,
                    Message = Format(Strings.ConfirmRemoveGroupMessage, CyRuleEdits.CountConditions(group)),
                    ConfirmText = Strings.ConfirmRemove,
                    Destructive = true
                },
                CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // No CyConfirmDialog host on the page: remove without asking.
            return true;
        }
    }

    private Task DuplicateAsync(string id) =>
        TryEditAsync(
            root => CyRuleEdits.Duplicate(root, id, EffectiveLimits),
            (_, _) => Strings.Duplicated,
            (_, next) =>
            {
                var parent = CyRuleEdits.FindParent(next, id);
                var index = parent?.Children.ToList().FindIndex(c => string.Equals(c.Id, id, StringComparison.Ordinal)) ?? -1;
                return parent is not null && index >= 0 && index + 1 < parent.Children.Count
                    ? FirstControlId(parent.Children[index + 1])
                    : null;
            });

    private Task WrapAsync(string id) =>
        TryEditAsync(
            root => CyRuleEdits.Wrap(root, id, EffectiveLimits),
            (_, _) => Strings.Wrapped,
            (before, _) => CyRuleEdits.Find(before, id) is { } node ? FirstControlId(node) : null);

    private Task UngroupAsync(string id) =>
        TryEditAsync(
            root => CyRuleEdits.Ungroup(root, id),
            (_, _) => Strings.Ungrouped,
            (before, _) =>
            {
                if (CyRuleEdits.Find(before, id) is not CyRuleGroup group)
                {
                    return null;
                }

                return group.Children.Count > 0
                    ? FirstControlId(group.Children[0])
                    : CyRuleEdits.FindParent(before, id) is { } parent ? Eid(parent, "add") : null;
            });

    private Task MoveToAsync(string id, string targetId) =>
        TryEditAsync(
            root => CyRuleEdits.MoveTo(root, id, targetId, int.MaxValue, EffectiveLimits),
            (_, next) => Format(Strings.Moved, CyRuleEdits.Find(next, targetId) is CyRuleGroup target ? TargetName(target) : string.Empty),
            (before, _) => CyRuleEdits.Find(before, id) is { } node ? FirstControlId(node) : null);

    private Task MoveByAsync(string id, int step)
    {
        var parent = CyRuleEdits.FindParent(_root, id);

        if (parent is null)
        {
            return Task.CompletedTask;
        }

        var index = parent.Children.ToList().FindIndex(c => string.Equals(c.Id, id, StringComparison.Ordinal));
        var target = index + step;
        var parentId = parent.Id;
        var node = parent.Children[index];

        return TryEditAsync(
            root => CyRuleEdits.Reorder(root, parentId, index, target),
            (_, next) => Format(Strings.Moved, CyRuleEdits.Find(next, parentId) is CyRuleGroup moved ? TargetName(moved) : string.Empty),
            (_, next) =>
            {
                var count = (CyRuleEdits.Find(next, parentId) as CyRuleGroup)?.Children.Count ?? 0;
                var atEdge = step < 0 ? target == 0 : target == count - 1;
                var button = (step < 0) == atEdge ? "down" : "up";
                return Eid(node, button);
            });
    }

    // ------------------------------------------------------------------ focus

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = _pendingFocus;

        if (target is null)
        {
            return;
        }

        _pendingFocus = null;

        if (Services.GetService(typeof(IFocusManager)) is not IFocusManager focus)
        {
            return;
        }

        try
        {
            await focus.FocusAsync(target);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone, or the element was removed: nothing to focus.
        }
    }
}
