# CymruBlazor design tokens

> **Generated** by `scripts/GenerateTokens.cs` from `wwwroot/css/{tokens,themes}`. Do not edit by hand;
> run `dotnet run scripts/GenerateTokens.cs` after changing token CSS.
>
> Machine-readable copies ship in the package: `_content/CymruBlazor/tokens/tokens.json` and `tokens.d.ts`.

## Using tokens correctly

- Always use a name from this page. A misspelt name inside `var(--x, #fallback)` silently renders the
  fallback, which then ignores dark and high-contrast themes.
- Spelling: tokens use **`color`**. Every `--cymru-color-*` token also has a `--cymru-colour-*` alias.
- Prefer **semantic** tokens (`--cymru-color-surface`, `--cymru-color-text`) over primitives
  (`--cymru-blue-800`). Semantic tokens flip with the theme; primitives never do.
- A `Dark` / `High contrast` cell shows where a theme overrides the value. A blank cell means the theme inherits the base value.

## alias

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-color-danger-border` | `var(--cymru-color-danger)` |  |  |
| `--cymru-color-info-border` | `var(--cymru-color-info)` |  |  |
| `--cymru-color-info-text` | `var(--cymru-color-info)` |  |  |
| `--cymru-color-success-border` | `var(--cymru-color-success)` |  |  |
| `--cymru-color-surface-hover` | `color-mix( in srgb, var(--cymru-color-surface), var(--cymru-color-text) 6% )` |  |  |
| `--cymru-color-surface-subtle` | `var(--cymru-color-surface-alt)` |  |  |
| `--cymru-color-text-secondary` | `var(--cymru-color-text-muted)` |  |  |
| `--cymru-color-warning-border` | `var(--cymru-color-warning)` |  |  |
| `--cymru-color-warning-text` | `var(--cymru-color-text)` |  |  |
| `--cymru-focus-ring-color` | `var(--cymru-color-focus-outline)` |  |  |
| `--cymru-focus-ring-offset` | `2px` |  |  |
| `--cymru-focus-ring-width` | `var(--cymru-border-width-focus)` |  |  |
| `--cymru-font-family-mono` | `var(--cymru-font-family-monospace)` |  |  |
| `--cymru-font-mono` | `var(--cymru-font-family-monospace)` |  |  |
| `--cymru-page-max-width-default` | `var(--cymru-container-lg)` |  |  |
| `--cymru-page-max-width-full` | `none` |  |  |
| `--cymru-page-max-width-narrow` | `var(--cymru-container-md)` |  |  |
| `--cymru-page-max-width-wide` | `var(--cymru-container-xl)` |  |  |
| `--cymru-radius-full` | `var(--cymru-radius-pill)` |  |  |
| `--cymru-shadow-2xl` | `0 16px 40px rgb(27 41 74 / 0.28)` |  |  |

## breakpoints

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-breakpoint-2xl-min` | `96rem` |  |  |
| `--cymru-breakpoint-desktop` | `64rem` |  |  |
| `--cymru-breakpoint-desktop-max` | `79.9375rem` |  |  |
| `--cymru-breakpoint-desktop-min` | `64rem` |  |  |
| `--cymru-breakpoint-large-max` | `89.9375rem` |  |  |
| `--cymru-breakpoint-large-min` | `80rem` |  |  |
| `--cymru-breakpoint-mobile` | `48rem` |  |  |
| `--cymru-breakpoint-mobile-max` | `47.9375rem` |  |  |
| `--cymru-breakpoint-tablet` | `48rem` |  |  |
| `--cymru-breakpoint-tablet-max` | `63.9375rem` |  |  |
| `--cymru-breakpoint-tablet-min` | `48rem` |  |  |
| `--cymru-breakpoint-wide` | `90rem` |  |  |
| `--cymru-breakpoint-xlarge-min` | `90rem` |  |  |
| `--cymru-container-2xl` | `var(--cymru-breakpoint-2xl-min)` |  |  |
| `--cymru-container-lg` | `var(--cymru-breakpoint-desktop-min)` |  |  |
| `--cymru-container-md` | `var(--cymru-max-width-body-text)` |  |  |
| `--cymru-container-sm` | `var(--cymru-max-width-form)` |  |  |
| `--cymru-container-xl` | `var(--cymru-max-width-page)` |  |  |
| `--cymru-epr-sidebar-width` | `15.5rem` |  |  |
| `--cymru-grid-columns-desktop` | `12` |  |  |
| `--cymru-grid-columns-mobile` | `4` |  |  |
| `--cymru-grid-columns-tablet` | `8` |  |  |
| `--cymru-gutter-desktop` | `var(--cymru-space-6)` |  |  |
| `--cymru-gutter-mobile` | `var(--cymru-space-4)` |  |  |
| `--cymru-gutter-tablet` | `var(--cymru-space-6)` |  |  |
| `--cymru-gutter-xlarge` | `var(--cymru-space-8)` |  |  |
| `--cymru-margin-desktop` | `var(--cymru-space-10)` |  |  |
| `--cymru-margin-large` | `var(--cymru-space-16)` |  |  |
| `--cymru-margin-mobile` | `var(--cymru-space-4)` |  |  |
| `--cymru-margin-tablet` | `var(--cymru-space-8)` |  |  |
| `--cymru-margin-xlarge` | `var(--cymru-space-20)` |  |  |
| `--cymru-max-width-body-text` | `45rem` |  |  |
| `--cymru-max-width-form` | `35rem` |  |  |
| `--cymru-max-width-page` | `87.5rem` |  |  |
| `--cymru-reading-width` | `75ch` |  |  |

## colour (primitive)

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-blue-100` | `#eceef3` |  |  |
| `--cymru-blue-200` | `#d4d8e2` |  |  |
| `--cymru-blue-300` | `#aab1c6` |  |  |
| `--cymru-blue-400` | `#828dac` |  |  |
| `--cymru-blue-50` | `#f4f5f8` |  |  |
| `--cymru-blue-500` | `#5c6991` |  |  |
| `--cymru-blue-600` | `#4c72ae` |  |  |
| `--cymru-blue-700` | `#3d6199` |  |  |
| `--cymru-blue-800` | `#325083` |  |  |
| `--cymru-blue-900` | `#1e3050` |  |  |
| `--cymru-cyan-100` | `#ebf5fa` |  |  |
| `--cymru-cyan-300` | `#d6eaf2` |  |  |
| `--cymru-cyan-400` | `#afd4e5` |  |  |
| `--cymru-cyan-50` | `#f4fafc` |  |  |
| `--cymru-cyan-500` | `#8dc0da` |  |  |
| `--cymru-cyan-600` | `#71accd` |  |  |
| `--cymru-cyan-700` | `#12a3c9` |  |  |
| `--cymru-cyan-800` | `#0d8bad` |  |  |
| `--cymru-cyan-900` | `#0a6a84` |  |  |
| `--cymru-focus-yellow` | `#ffeb3b` |  |  |
| `--cymru-green-100` | `#d9efe5` |  |  |
| `--cymru-green-600` | `#007f3b` |  |  |
| `--cymru-green-700` | `#005a28` |  |  |
| `--cymru-grey-100` | `#f0f4f5` |  |  |
| `--cymru-grey-200` | `#d8dde0` |  |  |
| `--cymru-grey-300` | `#c6cdd1` |  |  |
| `--cymru-grey-400` | `#aeb7bd` |  |  |
| `--cymru-grey-50` | `#f7fafa` |  |  |
| `--cymru-grey-500` | `#768692` |  |  |
| `--cymru-grey-600` | `#4c6272` |  |  |
| `--cymru-grey-700` | `#3b4e5b` |  |  |
| `--cymru-grey-800` | `#2c3a44` |  |  |
| `--cymru-grey-900` | `#212b32` |  |  |
| `--cymru-info-blue-100` | `#d6e8f5` |  |  |
| `--cymru-info-blue-700` | `#005aa8` |  |  |
| `--cymru-navy-100` | `#cdcfd6` |  |  |
| `--cymru-navy-300` | `#9ea1af` |  |  |
| `--cymru-navy-500` | `#707488` |  |  |
| `--cymru-navy-700` | `#464c64` |  |  |
| `--cymru-navy-900` | `#1b294a` |  |  |
| `--cymru-red-100` | `#fcdbd9` |  |  |
| `--cymru-red-600` | `#d5281b` |  |  |
| `--cymru-red-700` | `#8a1e16` |  |  |
| `--cymru-white` | `#ffffff` |  |  |
| `--cymru-yellow-100` | `#fdf6dc` |  |  |
| `--cymru-yellow-500` | `#f8ca4d` |  |  |

## colour (semantic)

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-color-accent` | `var(--cymru-navy-900)` | `var(--cymru-navy-900)` |  |
| `--cymru-color-accent-text` | `var(--cymru-white)` |  |  |
| `--cymru-color-background` | `var(--cymru-grey-100)` | `var(--cymru-grey-900)` | `#000000` |
| `--cymru-color-border` | `var(--cymru-grey-200)` | `var(--cymru-navy-500)` | `#ffffff` |
| `--cymru-color-border-strong` | `var(--cymru-grey-600)` | `var(--cymru-navy-300)` | `#ffffff` |
| `--cymru-color-brand-accent` | `var(--cymru-cyan-700)` |  |  |
| `--cymru-color-danger` | `var(--cymru-red-600)` | `var(--cymru-red-100)` | `#ff4040` |
| `--cymru-color-danger-background` | `var(--cymru-red-100)` | `var(--cymru-blue-900)` | `#000000` |
| `--cymru-color-danger-text` | `var(--cymru-red-700)` | `var(--cymru-red-100)` | `#ff4040` |
| `--cymru-color-disabled` | `var(--cymru-grey-300)` |  |  |
| `--cymru-color-disabled-background` | `var(--cymru-grey-100)` |  |  |
| `--cymru-color-disabled-text` | `var(--cymru-grey-500)` |  |  |
| `--cymru-color-focus` | `var(--cymru-focus-yellow)` |  | `#ffff00` |
| `--cymru-color-focus-outline` | `var(--cymru-focus-yellow)` |  | `#ffffff` |
| `--cymru-color-focus-text` | `var(--cymru-grey-900)` |  |  |
| `--cymru-color-info` | `var(--cymru-info-blue-700)` | `var(--cymru-cyan-300)` | `#00ffff` |
| `--cymru-color-info-background` | `var(--cymru-info-blue-100)` | `var(--cymru-navy-900)` | `#000000` |
| `--cymru-color-link` | `var(--cymru-info-blue-700)` | `var(--cymru-cyan-400)` | `#00ffff` |
| `--cymru-color-link-hover` | `var(--cymru-navy-900)` | `var(--cymru-cyan-300)` | `#ffffff` |
| `--cymru-color-link-visited` | `var(--cymru-blue-600)` |  | `#ffff00` |
| `--cymru-color-primary` | `var(--cymru-blue-800)` | `var(--cymru-cyan-900)` | `#00ffff` |
| `--cymru-color-primary-hover` | `var(--cymru-blue-900)` | `var(--cymru-cyan-800)` | `#ffffff` |
| `--cymru-color-primary-on-surface` | `var(--cymru-color-primary)` | `var(--cymru-cyan-400)` | `#00ffff` |
| `--cymru-color-primary-subtle` | `var(--cymru-blue-100)` |  | `#000000` |
| `--cymru-color-primary-text` | `var(--cymru-white)` | `var(--cymru-white)` | `#000000` |
| `--cymru-color-secondary` | `var(--cymru-navy-900)` | `var(--cymru-blue-300)` | `#ffff00` |
| `--cymru-color-selection` | `var(--cymru-blue-100)` |  |  |
| `--cymru-color-success` | `var(--cymru-green-600)` | `var(--cymru-green-100)` | `#00ff00` |
| `--cymru-color-success-background` | `var(--cymru-green-100)` | `var(--cymru-navy-900)` | `#000000` |
| `--cymru-color-success-text` | `var(--cymru-green-700)` | `var(--cymru-green-100)` | `#00ff00` |
| `--cymru-color-surface` | `var(--cymru-white)` | `var(--cymru-blue-900)` | `#000000` |
| `--cymru-color-surface-accent` | `var(--cymru-cyan-100)` | `var(--cymru-blue-900)` |  |
| `--cymru-color-surface-alt` | `var(--cymru-blue-50)` | `var(--cymru-navy-700)` | `#000000` |
| `--cymru-color-text` | `var(--cymru-grey-900)` | `var(--cymru-white)` | `#ffffff` |
| `--cymru-color-text-inverse` | `var(--cymru-white)` | `var(--cymru-white)` | `#000000` |
| `--cymru-color-text-muted` | `var(--cymru-grey-600)` | `var(--cymru-grey-200)` | `#ffffff` |
| `--cymru-color-warning` | `var(--cymru-yellow-500)` |  | `#ffff00` |
| `--cymru-color-warning-background` | `var(--cymru-yellow-100)` | `var(--cymru-navy-700)` | `#000000` |

## elevation

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-border-width-control` | `var(--cymru-border-width-thin)` |  |  |
| `--cymru-border-width-divider` | `var(--cymru-border-width-thin)` |  |  |
| `--cymru-border-width-focus` | `var(--cymru-border-width-medium)` |  |  |
| `--cymru-border-width-medium` | `2px` |  |  |
| `--cymru-border-width-none` | `0` |  |  |
| `--cymru-border-width-thick` | `3px` |  |  |
| `--cymru-border-width-thin` | `1px` |  |  |
| `--cymru-elevation-card` | `var(--cymru-shadow-sm)` |  |  |
| `--cymru-elevation-dialog` | `var(--cymru-shadow-xl)` |  |  |
| `--cymru-elevation-dropdown` | `var(--cymru-shadow-md)` |  |  |
| `--cymru-elevation-popover` | `var(--cymru-shadow-lg)` |  |  |
| `--cymru-elevation-surface` | `var(--cymru-shadow-none)` |  |  |
| `--cymru-radius-badge` | `var(--cymru-radius-pill)` |  |  |
| `--cymru-radius-card` | `var(--cymru-radius-xl)` |  |  |
| `--cymru-radius-circle` | `50%` |  |  |
| `--cymru-radius-control` | `var(--cymru-radius-sm)` |  |  |
| `--cymru-radius-dialog` | `var(--cymru-radius-lg)` |  |  |
| `--cymru-radius-lg` | `0.75rem` |  |  |
| `--cymru-radius-md` | `0.5rem` |  |  |
| `--cymru-radius-none` | `0` |  |  |
| `--cymru-radius-pill` | `9999px` |  |  |
| `--cymru-radius-sm` | `0.25rem` |  |  |
| `--cymru-radius-xl` | `1rem` |  |  |
| `--cymru-radius-xs` | `0.125rem` |  |  |
| `--cymru-shadow-button` | `0 2px 0 var(--cymru-navy-900)` |  |  |
| `--cymru-shadow-lg` | `var(--cymru-shadow-overlay)` |  |  |
| `--cymru-shadow-md` | `0 2px 8px rgb(27 41 74 / 0.15)` |  |  |
| `--cymru-shadow-none` | `none` |  |  |
| `--cymru-shadow-overlay` | `0 4px 16px rgb(27 41 74 / 0.18)` |  |  |
| `--cymru-shadow-raised` | `0 1px 4px rgb(27 41 74 / 0.12)` |  |  |
| `--cymru-shadow-sm` | `var(--cymru-shadow-raised)` |  |  |
| `--cymru-shadow-xl` | `0 8px 24px rgb(27 41 74 / 0.22)` |  |  |
| `--cymru-shadow-xs` | `0 1px 2px rgb(27 41 74 / 0.08)` |  |  |
| `--cymru-z-base` | `0` |  |  |
| `--cymru-z-dropdown` | `1000` |  |  |
| `--cymru-z-fixed` | `1030` |  |  |
| `--cymru-z-modal` | `1050` |  |  |
| `--cymru-z-overlay` | `1040` |  |  |
| `--cymru-z-popover` | `1060` |  |  |
| `--cymru-z-sticky` | `1020` |  |  |
| `--cymru-z-toast` | `1080` |  |  |
| `--cymru-z-tooltip` | `1070` |  |  |

## spacing

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-button-gap` | `var(--cymru-space-2)` |  |  |
| `--cymru-button-padding-block` | `var(--cymru-space-2)` |  |  |
| `--cymru-button-padding-inline` | `var(--cymru-space-4)` |  |  |
| `--cymru-card-gap` | `var(--cymru-space-4)` |  |  |
| `--cymru-card-padding` | `var(--cymru-space-6)` |  |  |
| `--cymru-container-padding-block` | `var(--cymru-space-6)` |  |  |
| `--cymru-container-padding-inline` | `var(--cymru-space-4)` |  |  |
| `--cymru-control-gap` | `var(--cymru-space-2)` |  |  |
| `--cymru-control-padding-block` | `var(--cymru-space-2)` |  |  |
| `--cymru-control-padding-inline` | `var(--cymru-space-4)` |  |  |
| `--cymru-dialog-padding` | `var(--cymru-space-8)` |  |  |
| `--cymru-focus-offset` | `2px` |  |  |
| `--cymru-layout-gap-lg` | `var(--cymru-space-6)` |  |  |
| `--cymru-layout-gap-md` | `var(--cymru-space-4)` |  |  |
| `--cymru-layout-gap-sm` | `var(--cymru-space-2)` |  |  |
| `--cymru-layout-gap-xl` | `var(--cymru-space-8)` |  |  |
| `--cymru-layout-gap-xs` | `var(--cymru-space-1)` |  |  |
| `--cymru-list-gap` | `var(--cymru-space-2)` |  |  |
| `--cymru-margin-lg` | `var(--cymru-space-6)` |  |  |
| `--cymru-margin-md` | `var(--cymru-space-4)` |  |  |
| `--cymru-margin-sm` | `var(--cymru-space-2)` |  |  |
| `--cymru-margin-xl` | `var(--cymru-space-8)` |  |  |
| `--cymru-margin-xs` | `var(--cymru-space-1)` |  |  |
| `--cymru-padding-lg` | `var(--cymru-space-6)` |  |  |
| `--cymru-padding-md` | `var(--cymru-space-4)` |  |  |
| `--cymru-padding-sm` | `var(--cymru-space-2)` |  |  |
| `--cymru-padding-xl` | `var(--cymru-space-8)` |  |  |
| `--cymru-padding-xs` | `var(--cymru-space-1)` |  |  |
| `--cymru-section-spacing` | `var(--cymru-space-12)` |  |  |
| `--cymru-space-0` | `0` |  |  |
| `--cymru-space-1` | `0.25rem` |  |  |
| `--cymru-space-10` | `2.5rem` |  |  |
| `--cymru-space-12` | `3rem` |  |  |
| `--cymru-space-16` | `4rem` |  |  |
| `--cymru-space-2` | `0.5rem` |  |  |
| `--cymru-space-20` | `5rem` |  |  |
| `--cymru-space-24` | `6rem` |  |  |
| `--cymru-space-3` | `0.75rem` |  |  |
| `--cymru-space-4` | `1rem` |  |  |
| `--cymru-space-5` | `1.25rem` |  |  |
| `--cymru-space-6` | `1.5rem` |  |  |
| `--cymru-space-8` | `2rem` |  |  |
| `--cymru-spacing-component-lg` | `var(--cymru-space-6)` |  |  |
| `--cymru-spacing-component-md` | `var(--cymru-space-4)` |  |  |
| `--cymru-spacing-component-sm` | `var(--cymru-space-2)` |  |  |
| `--cymru-spacing-component-xl` | `var(--cymru-space-8)` |  |  |
| `--cymru-spacing-component-xs` | `var(--cymru-space-1)` |  |  |
| `--cymru-spacing-form-error-gap` | `var(--cymru-space-1)` |  |  |
| `--cymru-spacing-form-field-gap` | `var(--cymru-space-6)` |  |  |
| `--cymru-spacing-form-group-gap` | `var(--cymru-space-8)` |  |  |
| `--cymru-spacing-form-hint-gap` | `var(--cymru-space-1)` |  |  |
| `--cymru-spacing-form-label-gap` | `var(--cymru-space-2)` |  |  |
| `--cymru-spacing-layout-md` | `var(--cymru-space-8)` |  |  |
| `--cymru-spacing-layout-page` | `var(--cymru-space-16)` |  |  |
| `--cymru-spacing-layout-section` | `var(--cymru-space-12)` |  |  |
| `--cymru-spacing-layout-sm` | `var(--cymru-space-6)` |  |  |
| `--cymru-spacing-layout-xs` | `var(--cymru-space-4)` |  |  |
| `--cymru-touch-target-min` | `2.75rem` |  |  |

## typography

| Token | Value | Dark | High contrast |
| --- | --- | --- | --- |
| `--cymru-font-family` | `var(--cymru-font-family-base)` |  |  |
| `--cymru-font-family-base` | `"Roboto", Arial, Helvetica, sans-serif` |  |  |
| `--cymru-font-family-monospace` | `"Cascadia Code", "Consolas", "Courier New", monospace` |  |  |
| `--cymru-font-size` | `var(--cymru-font-size-body)` |  |  |
| `--cymru-font-size-2xl` | `var(--cymru-font-size-scale-27)` |  |  |
| `--cymru-font-size-3xl` | `var(--cymru-font-size-scale-36)` |  |  |
| `--cymru-font-size-body` | `var(--cymru-font-size-scale-16)` |  |  |
| `--cymru-font-size-body-small` | `var(--cymru-font-size-scale-14)` |  |  |
| `--cymru-font-size-caption` | `var(--cymru-font-size-scale-12)` |  |  |
| `--cymru-font-size-heading-1` | `var(--cymru-font-size-scale-32)` |  |  |
| `--cymru-font-size-heading-2` | `var(--cymru-font-size-scale-27)` |  |  |
| `--cymru-font-size-heading-3` | `var(--cymru-font-size-scale-22)` |  |  |
| `--cymru-font-size-heading-4` | `var(--cymru-font-size-scale-19)` |  |  |
| `--cymru-font-size-heading-5` | `var(--cymru-font-size-scale-16)` |  |  |
| `--cymru-font-size-heading-6` | `var(--cymru-font-size-scale-16)` |  |  |
| `--cymru-font-size-label` | `var(--cymru-font-size-scale-14)` |  |  |
| `--cymru-font-size-lg` | `var(--cymru-font-size-scale-19)` |  |  |
| `--cymru-font-size-md` | `var(--cymru-font-size-scale-16)` |  |  |
| `--cymru-font-size-scale-12` | `0.75rem` |  |  |
| `--cymru-font-size-scale-14` | `0.875rem` |  |  |
| `--cymru-font-size-scale-16` | `1rem` |  |  |
| `--cymru-font-size-scale-19` | `1.1875rem` |  |  |
| `--cymru-font-size-scale-22` | `1.375rem` |  |  |
| `--cymru-font-size-scale-26` | `1.625rem` |  |  |
| `--cymru-font-size-scale-27` | `1.6875rem` |  |  |
| `--cymru-font-size-scale-32` | `2rem` |  |  |
| `--cymru-font-size-scale-36` | `2.25rem` |  |  |
| `--cymru-font-size-scale-48` | `3rem` |  |  |
| `--cymru-font-size-scale-64` | `4rem` |  |  |
| `--cymru-font-size-sm` | `var(--cymru-font-size-scale-14)` |  |  |
| `--cymru-font-size-xl` | `var(--cymru-font-size-scale-22)` |  |  |
| `--cymru-font-size-xs` | `var(--cymru-font-size-scale-12)` |  |  |
| `--cymru-font-weight` | `var(--cymru-font-weight-regular)` |  |  |
| `--cymru-font-weight-bold` | `700` |  |  |
| `--cymru-font-weight-medium` | `500` |  |  |
| `--cymru-font-weight-regular` | `400` |  |  |
| `--cymru-font-weight-semibold` | `700` |  |  |
| `--cymru-letter-spacing-caption` | `0.24px` |  |  |
| `--cymru-letter-spacing-default` | `0` |  |  |
| `--cymru-letter-spacing-label` | `0.7px` |  |  |
| `--cymru-letter-spacing-normal` | `0` |  |  |
| `--cymru-letter-spacing-tight` | `-0.02em` |  |  |
| `--cymru-letter-spacing-wide` | `0.02em` |  |  |
| `--cymru-line-height` | `var(--cymru-line-height-body)` |  |  |
| `--cymru-line-height-body` | `1.5` |  |  |
| `--cymru-line-height-body-size` | `1.5` |  |  |
| `--cymru-line-height-body-small` | `1.71429` |  |  |
| `--cymru-line-height-caption` | `1.33333` |  |  |
| `--cymru-line-height-heading` | `1.25` |  |  |
| `--cymru-line-height-heading-1` | `1.21875` |  |  |
| `--cymru-line-height-heading-2` | `1.22222` |  |  |
| `--cymru-line-height-heading-3` | `1.31818` |  |  |
| `--cymru-line-height-heading-4` | `1.42105` |  |  |
| `--cymru-line-height-heading-5` | `1.5` |  |  |
| `--cymru-line-height-heading-6` | `1.5` |  |  |
| `--cymru-line-height-label` | `1.42857` |  |  |
| `--cymru-line-height-relaxed` | `1.7` |  |  |
| `--cymru-line-height-tight` | `1.2` |  |  |

