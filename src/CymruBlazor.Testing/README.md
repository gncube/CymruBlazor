# CymruBlazor.Testing

bUnit helpers for testing apps built with [CymruBlazor](https://github.com/gncube/CymruBlazor). No browser needed.

```csharp
using CymruBlazor.Testing;

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

- **`CymruTestContext`**: a bUnit context with `AddCymruBlazor()` called, JavaScript interop in loose mode and the
  library's script modules set up. Derive your test classes from it.
- **`CymruMarkupChecks`**: structural accessibility checks on rendered markup: unique ids, `aria-labelledby`,
  `aria-describedby` and `label for` targets that exist, an accessible name on every form control and button, and
  heading levels that do not skip. `FindMarkupProblems()` returns the messages; `ShouldHaveNoMarkupProblems()` throws
  `CymruMarkupAssertionException` listing them.

These checks complement axe-core, they do not replace it: they cannot see colour, contrast, focus or layout. For a real
browser scan, see the companion package `CymruBlazor.Testing.Axe` and the
[testing guide](https://github.com/gncube/CymruBlazor/blob/main/docs/Testing.md).

The package version always matches the CymruBlazor version it was built against.
