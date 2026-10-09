namespace CymruBlazor.Components.Forms;

/// <summary>Size limits for a rule. <see cref="Unlimited"/> turns them all off.</summary>
/// <param name="MaxDepth">Deepest allowed nesting of groups, counting the root as 1.</param>
/// <param name="MaxConditions">Most conditions allowed in the whole rule.</param>
/// <param name="MinConditions">Fewest conditions that removing may leave.</param>
public sealed record CyRuleLimits(int MaxDepth = 3, int MaxConditions = 50, int MinConditions = 0)
{
    /// <summary>No limits.</summary>
    public static CyRuleLimits Unlimited { get; } = new(int.MaxValue, int.MaxValue, 0);
}

/// <summary>
/// Pure edits of a rule tree. Every method returns the new root. When an edit is not possible (unknown id, a limit,
/// moving a group into itself) the <b>same root instance</b> is returned, so callers can test
/// <see cref="object.ReferenceEquals(object, object)"/> to know whether anything changed.
/// </summary>
public static class CyRuleEdits
{
    /// <summary>The node with this id anywhere in the tree (the root included), or <see langword="null"/>.</summary>
    public static CyRuleNode? Find(CyRuleGroup root, string id)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (string.Equals(root.Id, id, StringComparison.Ordinal))
        {
            return root;
        }

        foreach (var child in root.Children)
        {
            if (string.Equals(child.Id, id, StringComparison.Ordinal))
            {
                return child;
            }

            if (child is CyRuleGroup group && Find(group, id) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>The group that directly contains this node, or <see langword="null"/> for the root or an unknown id.</summary>
    public static CyRuleGroup? FindParent(CyRuleGroup root, string id)
    {
        ArgumentNullException.ThrowIfNull(root);

        foreach (var child in root.Children)
        {
            if (string.Equals(child.Id, id, StringComparison.Ordinal))
            {
                return root;
            }

            if (child is CyRuleGroup group && FindParent(group, id) is { } parent)
            {
                return parent;
            }
        }

        return null;
    }

    /// <summary>The nesting level of a group (the root is 1), or 0 when the id is not a group in the tree.</summary>
    public static int Depth(CyRuleGroup root, string groupId)
    {
        ArgumentNullException.ThrowIfNull(root);

        return DepthCore(root, groupId, 1);
    }

    /// <summary>How many group levels this group spans, itself included (a group with no nested groups is 1).</summary>
    public static int LevelsBelow(CyRuleGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var deepest = 0;
        foreach (var child in group.Children)
        {
            if (child is CyRuleGroup nested)
            {
                deepest = Math.Max(deepest, LevelsBelow(nested));
            }
        }

        return 1 + deepest;
    }

    /// <summary>How many conditions are in the group, nested groups included.</summary>
    public static int CountConditions(CyRuleGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var count = 0;
        foreach (var child in group.Children)
        {
            count += child is CyRuleGroup nested ? CountConditions(nested) : 1;
        }

        return count;
    }

    /// <summary>Applies <paramref name="change"/> to the node with this id (the root included). Nothing else changes.</summary>
    public static CyRuleGroup Update(CyRuleGroup root, string id, Func<CyRuleNode, CyRuleNode> change)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(change);

        if (string.Equals(root.Id, id, StringComparison.Ordinal))
        {
            return change(root) is CyRuleGroup changedRoot ? changedRoot : root;
        }

        return Result(root, id, node => [change(node)]);
    }

    /// <summary>Sets how a group's children combine.</summary>
    public static CyRuleGroup SetCombinator(CyRuleGroup root, string groupId, CyRuleCombinator combinator) =>
        Update(root, groupId, node => node is CyRuleGroup g ? g with { Combinator = combinator } : node);

    /// <summary>Sets whether a group is negated.</summary>
    public static CyRuleGroup SetNot(CyRuleGroup root, string groupId, bool not) =>
        Update(root, groupId, node => node is CyRuleGroup g ? g with { Not = not } : node);

    /// <summary>Adds a condition (blank unless given) at the end of a group, or at <paramref name="index"/>.</summary>
    public static CyRuleGroup AddCondition(
        CyRuleGroup root,
        string groupId,
        CyRuleCondition? condition = null,
        int? index = null,
        CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var l = limits ?? CyRuleLimits.Unlimited;
        if (CountConditions(root) + 1 > l.MaxConditions)
        {
            return root;
        }

        return InsertInto(root, groupId, condition ?? new CyRuleCondition(), index);
    }

    /// <summary>Adds a new group, holding one blank condition, at the end of <paramref name="parentId"/>.</summary>
    public static CyRuleGroup AddGroup(CyRuleGroup root, string parentId, int? index = null, CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var l = limits ?? CyRuleLimits.Unlimited;
        var parentDepth = Depth(root, parentId);
        if (parentDepth == 0 || parentDepth + 1 > l.MaxDepth || CountConditions(root) + 1 > l.MaxConditions)
        {
            return root;
        }

        var group = new CyRuleGroup { Children = [new CyRuleCondition()] };
        return InsertInto(root, parentId, group, index);
    }

    /// <summary>Removes a condition or group. The root cannot be removed, and removing may not go below <c>MinConditions</c>.</summary>
    public static CyRuleGroup Remove(CyRuleGroup root, string id, CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var node = Find(root, id);
        if (node is null || string.Equals(root.Id, id, StringComparison.Ordinal))
        {
            return root;
        }

        var l = limits ?? CyRuleLimits.Unlimited;
        var removing = node is CyRuleGroup g ? CountConditions(g) : 1;
        if (CountConditions(root) - removing < l.MinConditions)
        {
            return root;
        }

        return Result(root, id, _ => []);
    }

    /// <summary>Copies a condition or group (with new ids) directly after the original.</summary>
    public static CyRuleGroup Duplicate(CyRuleGroup root, string id, CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var node = Find(root, id);
        if (node is null || string.Equals(root.Id, id, StringComparison.Ordinal))
        {
            return root;
        }

        var l = limits ?? CyRuleLimits.Unlimited;
        var adding = node is CyRuleGroup g ? CountConditions(g) : 1;
        if (CountConditions(root) + adding > l.MaxConditions)
        {
            return root;
        }

        return Result(root, id, original => [original, CloneWithNewIds(original)]);
    }

    /// <summary>Replaces a condition or group by a new "and" group that contains it.</summary>
    public static CyRuleGroup Wrap(CyRuleGroup root, string id, CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var node = Find(root, id);
        var parent = FindParent(root, id);
        if (node is null || parent is null)
        {
            return root;
        }

        var l = limits ?? CyRuleLimits.Unlimited;
        var below = node is CyRuleGroup g ? LevelsBelow(g) : 0;
        if (Depth(root, parent.Id) + 1 + below > l.MaxDepth)
        {
            return root;
        }

        return Result(root, id, original => [new CyRuleGroup { Children = [original] }]);
    }

    /// <summary>Removes a group but keeps its children, in its place. The root cannot be ungrouped.</summary>
    public static CyRuleGroup Ungroup(CyRuleGroup root, string groupId)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (Find(root, groupId) is not CyRuleGroup group || string.Equals(root.Id, groupId, StringComparison.Ordinal))
        {
            return root;
        }

        return Result(root, groupId, _ => group.Children);
    }

    /// <summary>
    /// Moves a node into another group. <paramref name="index"/> is the position among the target's children
    /// <b>after</b> the node has been taken out. A group cannot move into itself or its own descendants.
    /// </summary>
    public static CyRuleGroup MoveTo(
        CyRuleGroup root,
        string nodeId,
        string targetGroupId,
        int index,
        CyRuleLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        var node = Find(root, nodeId);
        if (node is null || string.Equals(root.Id, nodeId, StringComparison.Ordinal) ||
            Find(root, targetGroupId) is not CyRuleGroup)
        {
            return root;
        }

        if (node is CyRuleGroup moving && Find(moving, targetGroupId) is not null)
        {
            return root; // into itself or a descendant
        }

        var l = limits ?? CyRuleLimits.Unlimited;
        var below = node is CyRuleGroup g ? LevelsBelow(g) : 0;
        if (Depth(root, targetGroupId) + below > l.MaxDepth)
        {
            return root;
        }

        var without = Result(root, nodeId, _ => []);
        return InsertInto(without, targetGroupId, node, index);
    }

    /// <summary>Moves a node to a new position inside the group that already holds it.</summary>
    public static CyRuleGroup Reorder(CyRuleGroup root, string groupId, int oldIndex, int newIndex)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (Find(root, groupId) is not CyRuleGroup group ||
            oldIndex < 0 || oldIndex >= group.Children.Count ||
            newIndex < 0 || newIndex >= group.Children.Count ||
            oldIndex == newIndex)
        {
            return root;
        }

        var children = group.Children.ToList();
        var moved = children[oldIndex];
        children.RemoveAt(oldIndex);
        children.Insert(newIndex, moved);

        return Update(root, groupId, _ => group with { Children = children });
    }

    // ---- internals

    private static int DepthCore(CyRuleGroup group, string groupId, int level)
    {
        if (string.Equals(group.Id, groupId, StringComparison.Ordinal))
        {
            return level;
        }

        foreach (var child in group.Children)
        {
            if (child is CyRuleGroup nested && DepthCore(nested, groupId, level + 1) is var d and > 0)
            {
                return d;
            }
        }

        return 0;
    }

    private static CyRuleGroup InsertInto(CyRuleGroup root, string groupId, CyRuleNode node, int? index)
    {
        if (Find(root, groupId) is not CyRuleGroup target)
        {
            return root;
        }

        var children = target.Children.ToList();
        var at = index is { } i ? Math.Clamp(i, 0, children.Count) : children.Count;
        children.Insert(at, node);

        return Update(root, groupId, _ => target with { Children = children });
    }

    /// <summary>Rewrites the tree, replacing the child with this id by what <paramref name="replace"/> returns.</summary>
    private static CyRuleGroup Result(CyRuleGroup root, string id, Func<CyRuleNode, IEnumerable<CyRuleNode>> replace) =>
        Rewrite(root, id, replace, out var rewritten) ? rewritten : root;

    private static bool Rewrite(
        CyRuleGroup group,
        string id,
        Func<CyRuleNode, IEnumerable<CyRuleNode>> replace,
        out CyRuleGroup result)
    {
        var children = new List<CyRuleNode>(group.Children.Count);
        var found = false;

        foreach (var child in group.Children)
        {
            if (!found && string.Equals(child.Id, id, StringComparison.Ordinal))
            {
                children.AddRange(replace(child));
                found = true;
            }
            else if (!found && child is CyRuleGroup nested && Rewrite(nested, id, replace, out var rewrittenNested))
            {
                children.Add(rewrittenNested);
                found = true;
            }
            else
            {
                children.Add(child);
            }
        }

        result = found ? group with { Children = children } : group;
        return found;
    }

    private static CyRuleNode CloneWithNewIds(CyRuleNode node) => node switch
    {
        CyRuleGroup group => group with
        {
            Id = CyRuleNode.NewId(),
            Children = group.Children.Select(CloneWithNewIds).ToList()
        },
        CyRuleCondition condition => condition with { Id = CyRuleNode.NewId() },
        _ => node
    };
}
