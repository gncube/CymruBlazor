namespace CymruBlazor.Components.Forms;

/// <summary>
/// Every phrase <c>CyRuleBuilder</c> shows, speaks or uses to describe a rule, as format strings, so a
/// Welsh (or any other) service can replace them. Unset properties keep their English defaults.
/// </summary>
/// <remarks>
/// Pass an instance to the <c>Text</c> parameter: <c>Text="@(new CyRuleBuilderText { AddCondition = "Ychwanegu amod" })"</c>.
/// The summary formats (<see cref="Condition0"/> to <see cref="Condition2"/>, <see cref="NotFormat"/>,
/// <see cref="GroupParens"/>) are whole sentences so word order can change; the joiners include their spaces.
/// </remarks>
public sealed record CyRuleBuilderText
{
    // ---- group phrases (legend of a group)

    /// <summary>Legend of an "and" group.</summary>
    public string GroupAll { get; init; } = "All of the following";

    /// <summary>Legend of an "or" group.</summary>
    public string GroupAny { get; init; } = "Any of the following";

    /// <summary>Legend of a negated "and" group.</summary>
    public string GroupNotAll { get; init; } = "Not all of the following";

    /// <summary>Legend of a negated "or" group.</summary>
    public string GroupNone { get; init; } = "None of the following";

    // ---- operator labels

    /// <summary>Operator: equal.</summary>
    public string OpEq { get; init; } = "is";

    /// <summary>Operator: not equal.</summary>
    public string OpNe { get; init; } = "is not";

    /// <summary>Operator: text contains.</summary>
    public string OpContains { get; init; } = "contains";

    /// <summary>Operator: starts with.</summary>
    public string OpStartsWith { get; init; } = "starts with";

    /// <summary>Operator: ends with.</summary>
    public string OpEndsWith { get; init; } = "ends with";

    /// <summary>Operator: greater than.</summary>
    public string OpGt { get; init; } = "is greater than";

    /// <summary>Operator: at least.</summary>
    public string OpGte { get; init; } = "is at least";

    /// <summary>Operator: less than.</summary>
    public string OpLt { get; init; } = "is less than";

    /// <summary>Operator: at most.</summary>
    public string OpLte { get; init; } = "is at most";

    /// <summary>Operator: date before.</summary>
    public string OpBefore { get; init; } = "is before";

    /// <summary>Operator: date after.</summary>
    public string OpAfter { get; init; } = "is after";

    /// <summary>Operator: between two values.</summary>
    public string OpBetween { get; init; } = "is between";

    /// <summary>Operator: has no value.</summary>
    public string OpIsEmpty { get; init; } = "is empty";

    /// <summary>Operator: has a value.</summary>
    public string OpIsNotEmpty { get; init; } = "is not empty";

    /// <summary>Operator: boolean yes.</summary>
    public string OpIsTrue { get; init; } = "is yes";

    /// <summary>Operator: boolean no.</summary>
    public string OpIsFalse { get; init; } = "is no";

    // ---- control labels

    /// <summary>Label of the field picker.</summary>
    public string FieldLabel { get; init; } = "Field";

    /// <summary>Label of the operator picker.</summary>
    public string OperatorLabel { get; init; } = "Operator";

    /// <summary>Label of the value editor.</summary>
    public string ValueLabel { get; init; } = "Value";

    /// <summary>Label of the second value of "between".</summary>
    public string ValueToLabel { get; init; } = "Upper value";

    /// <summary>First entry of the field and operator pickers.</summary>
    public string ChoosePlaceholder { get; init; } = "Choose...";

    /// <summary>Accessible name prefix of a condition's controls. {0} position, {1} total.</summary>
    public string ConditionPosition { get; init; } = "Condition {0} of {1}";

    /// <summary>Label of the combinator control.</summary>
    public string CombinatorLabel { get; init; } = "Match";

    /// <summary>Combinator choice: all conditions.</summary>
    public string MatchAll { get; init; } = "All";

    /// <summary>Combinator choice: any condition.</summary>
    public string MatchAny { get; init; } = "Any";

    /// <summary>Label of the switch that negates a group.</summary>
    public string NotLabel { get; init; } = "Negate this group";

    /// <summary>Button: add a condition.</summary>
    public string AddCondition { get; init; } = "Add condition";

    /// <summary>Button: add a group.</summary>
    public string AddGroup { get; init; } = "Add group";

    /// <summary>Button name: remove a condition. {0} position.</summary>
    public string RemoveCondition { get; init; } = "Remove condition {0}";

    /// <summary>Button name: remove a group. {0} the group's legend.</summary>
    public string RemoveGroup { get; init; } = "Remove group: {0}";

    /// <summary>Name of a row's actions menu button. {0} position.</summary>
    public string MoreActions { get; init; } = "More actions for condition {0}";

    /// <summary>Menu item: duplicate.</summary>
    public string Duplicate { get; init; } = "Duplicate";

    /// <summary>Menu item: wrap in a new group.</summary>
    public string WrapInGroup { get; init; } = "Wrap in group";

    /// <summary>Menu item: remove the group but keep its conditions.</summary>
    public string Ungroup { get; init; } = "Ungroup";

    /// <summary>Menu item: move to another group. {0} the target group's legend.</summary>
    public string MoveTo { get; init; } = "Move to: {0}";

    /// <summary>Name of the list of conditions in a group. {0} the group's legend.</summary>
    public string ConditionsList { get; init; } = "Conditions: {0}";

    /// <summary>Visible and spoken text when a group has no conditions.</summary>
    public string EmptyGroup { get; init; } = "No conditions in this group.";

    // ---- limits

    /// <summary>Hint when groups cannot nest deeper.</summary>
    public string MaxDepthReached { get; init; } = "Groups cannot be nested any deeper.";

    /// <summary>Hint when no more conditions can be added. {0} the limit.</summary>
    public string MaxConditionsReached { get; init; } = "The limit of {0} conditions has been reached.";

    /// <summary>Hint when the minimum number of conditions is reached.</summary>
    public string MinConditionsReached { get; init; } = "At least {0} condition is needed.";

    // ---- validation messages

    /// <summary>A condition has no field.</summary>
    public string IssueNoField { get; init; } = "Choose a field.";

    /// <summary>A condition has no operator.</summary>
    public string IssueNoOperator { get; init; } = "Choose an operator.";

    /// <summary>A condition needs a value.</summary>
    public string IssueMissingValue { get; init; } = "Enter a value.";

    /// <summary>The two values of "between" are the wrong way round.</summary>
    public string IssueInvalidRange { get; init; } = "The first value must not be greater than the second.";

    /// <summary>The chosen field is not in the field list.</summary>
    public string IssueUnknownField { get; init; } = "This field is no longer available.";

    /// <summary>The chosen operator is not offered for the field.</summary>
    public string IssueUnknownOperator { get; init; } = "This operator is not available for the chosen field.";

    // ---- announcements

    /// <summary>Announced after adding a condition. {0} its position.</summary>
    public string ConditionAdded { get; init; } = "Condition {0} added.";

    /// <summary>Announced after removing a condition.</summary>
    public string ConditionRemoved { get; init; } = "Condition removed.";

    /// <summary>Announced after adding a group.</summary>
    public string GroupAdded { get; init; } = "Group added.";

    /// <summary>Announced after removing a group.</summary>
    public string GroupRemoved { get; init; } = "Group removed.";

    /// <summary>Announced after duplicating.</summary>
    public string Duplicated { get; init; } = "Duplicated.";

    /// <summary>Announced after moving. {0} the target group's legend.</summary>
    public string Moved { get; init; } = "Moved to: {0}.";

    /// <summary>Announced after wrapping in a group.</summary>
    public string Wrapped { get; init; } = "Wrapped in a new group.";

    /// <summary>Announced after ungrouping.</summary>
    public string Ungrouped { get; init; } = "Group removed. Its conditions were kept.";

    /// <summary>Announced when changing the field reset the operator or value.</summary>
    public string FieldChanged { get; init; } = "Field changed. The operator and value were reset.";

    /// <summary>Announced when an action could not be done because of a limit.</summary>
    public string ActionNotAllowed { get; init; } = "That change is not allowed.";

    // ---- confirm dialog

    /// <summary>Title of the confirmation to remove a group that has conditions.</summary>
    public string ConfirmRemoveGroupTitle { get; init; } = "Remove this group?";

    /// <summary>Message of that confirmation. {0} number of conditions in the group.</summary>
    public string ConfirmRemoveGroupMessage { get; init; } = "The group and the {0} condition(s) in it will be removed.";

    /// <summary>Confirm button text of that dialog.</summary>
    public string ConfirmRemove { get; init; } = "Remove";

    // ---- summary

    /// <summary>Heading above the plain-language summary.</summary>
    public string SummaryHeading { get; init; } = "Summary";

    /// <summary>Summary when there are no conditions at all.</summary>
    public string SummaryEmpty { get; init; } = "No conditions yet.";

    /// <summary>Summary of a condition with no value. {0} field, {1} operator.</summary>
    public string Condition0 { get; init; } = "{0} {1}";

    /// <summary>Summary of a condition with one value. {0} field, {1} operator, {2} value.</summary>
    public string Condition1 { get; init; } = "{0} {1} {2}";

    /// <summary>Summary of a "between" condition. {0} field, {1} operator, {2} first value, {3} second value.</summary>
    public string Condition2 { get; init; } = "{0} {1} {2} and {3}";

    /// <summary>Joins the parts of an "and" group. Include the spaces.</summary>
    public string JoinAnd { get; init; } = " and ";

    /// <summary>Joins the parts of an "or" group. Include the spaces.</summary>
    public string JoinOr { get; init; } = " or ";

    /// <summary>Wraps a nested group of several parts. {0} the parts.</summary>
    public string GroupParens { get; init; } = "({0})";

    /// <summary>Negates a group in the summary. {0} the group.</summary>
    public string NotFormat { get; init; } = "not {0}";

    /// <summary>A group with no parts in the summary.</summary>
    public string SummaryEmptyGroup { get; init; } = "(no conditions)";

    /// <summary>A condition that is not complete, in the summary.</summary>
    public string SummaryIncomplete { get; init; } = "(incomplete condition)";
}
