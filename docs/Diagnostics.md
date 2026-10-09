# CymruBlazor diagnostics

CymruBlazor reports component misuse it can recover from through `ICyDiagnostics`. Each report has a stable code,
so you can search for it, filter it in your logging, and look it up here.

## Modes

Set the mode when you register the library:

```csharp
builder.Services.AddCymruBlazor(o =>
    o.Diagnostics = builder.HostEnvironment.IsDevelopment()
        ? CyDiagnosticsMode.Strict
        : CyDiagnosticsMode.Lenient);
```

| Mode | What a recoverable mistake does |
|---|---|
| `Strict` (default) | Throws, so a typo is loud in Development. |
| `Lenient` | Logs one warning and renders a safe fallback, so one mistake cannot take down a page in Production. |

Warnings go to `ILogger<CyDiagnostics>` at `Warning` level, once per distinct code and message per process. With no
logger registered nothing is logged. Without `AddCymruBlazor()` components behave as `Strict` and log nothing.

## Codes

| Code | Raised by | Meaning | Strict | Lenient |
|---|---|---|---|---|
| `CY0001` | `CyIcon` | The icon `Name` is not in `IconRegistry`. Check the spelling, or add the icon with `IconRegistry.Register`. | Throws `ArgumentException` | Warns, renders a placeholder |
| `CY0003` | `CyButton` | `IconOnly` is set but `Icon` is not, so the button would render nothing. | Throws `InvalidOperationException` | Warns |
| `CY0004` | `CyButton` | An icon-only button has no accessible name. Set `AriaLabel` or `AriaLabelledBy`. | Throws `InvalidOperationException` | Warns |
| `CY0005` | Form fields (`CyTextBox`, `CyTextArea`, `CySelect`, `CyCheckbox`, `CyRadioGroup`, `CyCheckboxGroup`, `CyDateInput`, `CyNumberInput`, `CyCombobox`, `CyMultiCombobox`, `CySwitch`) | The field has no `Label` and no non-blank `aria-label` or `aria-labelledby`, so assistive technology cannot name it. | **Warns** | Warns |

`CY0002` is not used.

### Why `CY0005` never throws

Fields rendered with an empty `Label` before 1.13.0 kept rendering, and an accessibility warning is not worth failing
a page that works. Fix it by setting `Label`. When the visible label really is elsewhere on the page (a table
header, say), give the field `aria-label` or `aria-labelledby` instead, which also silences the warning.

`CyFileUpload` is stricter and always throws for an empty `Label`; it is unchanged.
