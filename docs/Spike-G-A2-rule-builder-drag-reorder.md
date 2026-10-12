# Spike G-A2: drag reorder in `CyRuleBuilder`

Status: **report for your decision**. Read from the 1.12.3 source; nothing was built or run (no browser in the
sandbox), so every "works" below is a reading of the code, not a test result.

## Question

Can `CyRuleBuilder` reorder conditions and groups by dragging, reusing `CySortableList`'s pointer-drag module
(`wwwroot/js/sortable-list.js`), including moving a node into another group?

## What already exists

- **Model.** `CyRuleEdits.Reorder(root, groupId, oldIndex, newIndex)` and `CyRuleEdits.MoveTo(root, nodeId, targetGroupId, index, limits)`.
  `MoveTo`'s `index` is "position among the target's children after the node is taken out", which is exactly what
  `sortable-list.js` computes (`insertionIndex` ignores the dragged item). `MoveTo` already refuses a move into
  itself or a descendant and a move past `MaxDepth`, returning the root unchanged.
- **Keyboard and assistive technology.** Move up, move down and the "Move to" menu already exist on every row. They
  satisfy WCAG 2.5.7 (Dragging Movements), so dragging can be a pointer-only enhancement.
- **Markup.** Each group's children are a `ul.cy-rule__list > li.cy-rule__item`, with a nested ul inside the li for groups.

## Findings (what the JS does with nested lists)

| # | Finding | Consequence |
|---|---|---|
| 1 | Every registered list listens for `pointerdown` on its root. A press inside a nested list bubbles to the outer root too, but `startPress` returns when `item.parentElement !== state.root`, so only the innermost list starts the drag. | Nesting is safe for starting a drag. |
| 2 | `listUnderPointer` walks `document.elementsFromPoint` and accepts any list in the same group, **including lists inside the item being dragged**. | Dragging a group over its own children would show a drop indicator and then be silently refused by `MoveTo`. Needs a small JS change: skip any root the dragged item contains. |
| 3 | `OnPointerDrop(targetListId, oldIndex, newIndex)` does not say which list the drag **started** in. `CySortableList` is the source because it is the registered `this`. | The rule builder would register many lists against one .NET object, so the source is unknown. Needs an additive fourth argument (source list id); `CySortableList` ignores it. |
| 4 | `CyRuleBuilder` renders `<p class="cy-rule__empty">` instead of a `ul` when a group has no children. | An empty group cannot be a drop target. It needs an always-present (empty) list element, with the message kept as its visible content. |
| 5 | The ghost is a clone of the whole `li`. | Dragging a group clones the whole nested subtree. Acceptable, but clone only the node header for groups. |
| 6 | The drag handle in `CySortableList` is a focusable button with the keyboard pick-up protocol. | In the rule builder the keyboard path is the existing buttons and menu, so the handle should be a **non-focusable, aria-hidden** grip (`span`, `touch-action: none`). A focusable handle that does nothing for the keyboard would be an accessibility regression. |
| 7 | Drops refused by limits (depth, condition count) change nothing. | Announce a localised "cannot move there" message instead of staying silent (new `CyRuleBuilderText` property). |

## Verdict

**Feasible, medium effort, moderate risk.** The model needs no change. The work is:

1. `sortable-list.js`: skip descendants of the dragged item (finding 2) and pass the source list id (finding 3). Both additive.
2. `CyRuleBuilder`: opt-in `AllowDragReorder` (default `false`, so existing markup is unchanged), a grip per row, `data-cy-*` attributes on the lists and items, an always-rendered empty list for empty groups, a `[JSInvokable] OnPointerDrop` that maps list id to group id and calls `Reorder` or `MoveTo`, and module registration after render.
3. `CyRuleBuilderText`: `CannotMoveHere`. CSS for the grip and drop indicators (tokens only, three themes).
4. Tests. bUnit cannot drive pointer events, so the behaviour needs **Playwright tests** in `CymruBlazor.AccessibilityTests` (same pattern as `OverlayModuleBrowserTests`): reorder within a group, move into another group, drop into an empty group, a group refused when dropped on its own descendant, and a depth-limit refusal; plus axe with the grips present in three themes.

Risk: the first run of those browser tests is where surprises will show (pointer-capture timing, `elementsFromPoint` through sticky headers). Plan for one fix round.

Size: about the scale of hand-off 3 (one JS tweak, one component change, one test file with five browser tests).

## Decision needed

- **Build it** as one hand-off, `AllowDragReorder` opt-in, in 1.13.0 (recommended if drag is wanted at all); or
- **Defer it**: the keyboard and menu paths already cover the need, and drag is a pointer nicety. Nothing in this report is wasted; the findings above are the starting brief.
