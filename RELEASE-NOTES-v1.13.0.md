# CymruBlazor 1.13.0

A feature release. It is additive only: nothing is removed or changed, every new parameter defaults to the previous behaviour, and with the defaults the markup is unchanged. Upgrading from 1.12.x needs no code changes.

## Highlights

### Two new testing packages

Test your own pages against the same accessibility checks CymruBlazor uses on itself. Both packages are released with `CymruBlazor` at the same version.

```powershell
dotnet add package CymruBlazor.Testing        # bUnit helpers, no browser needed
dotnet add package CymruBlazor.Testing.Axe    # Playwright and axe-core scans in a real browser
```

**`CymruBlazor.Testing`**: `CymruTestContext` is a bUnit context with CymruBlazor registered, JavaScript interop in loose mode and the library's script modules set up. `CymruMarkupChecks` checks rendered markup for unique ids, `aria-labelledby`, `aria-describedby` and `label for` targets that exist, an accessible name on every form control and button, and heading levels that do not skip.

```csharp
public sealed class ReferralFormTests : CymruTestContext
{
    [Fact]
    public void The_form_markup_is_well_formed()
    {
        var cut = Render<ReferralForm>();

        cut.ShouldHaveNoMarkupProblems();
    }
}
```

**`CymruBlazor.Testing.Axe`**: `CymruAxeTestBase` renders a component, loads it with the library's own CSS into Chromium through Playwright and scans it with axe-core, so it finds contrast and focus problems that markup checks cannot see. Scan the light, dark and high-contrast themes; `AppCss` adds your own stylesheet.

```csharp
public sealed class ReferralFormAccessibilityTests : CymruAxeTestBase
{
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("high-contrast")]
    public async Task Referral_form_has_no_violations(string theme)
    {
        var result = await ScanComponentAsync<ReferralForm>(theme: theme);

        result.ShouldHaveNoViolations();
    }
}
```

Keep the Axe tests in a separate project from your bUnit tests: Playwright is large and needs a browser (`playwright.ps1 install chromium`). See [`docs/Testing.md`](docs/Testing.md).

### `CyRuleBuilder`: drag to reorder, and custom value editors

- **`AllowDragReorder`** (default false): drag a condition or group by its grip to reorder it or move it into another group, including an empty one. The grip works with mouse, touch and pen. The move buttons and "Move to" menu remain the keyboard way to do the same thing. A drop that is not allowed changes nothing and announces `CyRuleBuilderText.CannotMoveHere`.
- **`CyRuleField.ValueEditor`**: replace the built-in value editor for a field with your own. If your editor throws, a fixed message is shown, the exception is logged and never displayed, and the rest of the builder keeps working.

### Catch missing labels while you develop

New diagnostic **`CY0005`**: a form field rendered with no `Label` and no `aria-label` / `aria-labelledby` logs a warning once. It never throws, so no existing page starts failing. [`docs/Diagnostics.md`](docs/Diagnostics.md) lists every diagnostic code.

### `CyDataTable`: page size and sort events

- `PageSizeOptions` shows a labelled "Rows per page" select beside the pagination. Choosing a size returns to page 1, announces the change and raises `PageSizeChanged` (so `@bind-PageSize` works).
- `OnSortChanged` raises a `CyDataSortChange(Key, Descending)` after each sort change.

### Documentation

- [Testing guide](docs/Testing.md), including copyable base classes if you would rather not add a package.
- [Recipes](docs/Recipes.md): complete working pages for a list, an editor, a wizard and a tool workspace, a layout cookbook, and a localisation example. The Demo overview page is in English and Welsh.

## Upgrade notes

- No breaking changes. Package validation now compares against 1.12.3.
- The `CY0005` warning may appear in your logs for existing fields that have no accessible name. Fix them by adding a `Label` or `aria-label`.

## Quality

Unit tests 1352 of 1352. Accessibility tests 219 of 219, including a smoke run over every Demo route in the light, dark and high-contrast themes.

**Full changelog**: [`CHANGELOG.md`](CHANGELOG.md)
