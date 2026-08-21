// CymruBlazor theme interop.
//
// Deliberately minimal, per the project's "minimise JavaScript" principle
// (see PROMPT.md, "Technology Stack"): this is the one thing native Blazor
// genuinely cannot do on its own - read localStorage and the OS colour
// scheme preference before the .NET runtime has rendered anything.
//
// Consumers only need to reference this if they use <CyThemeProvider> with
// persistence/system-preference detection. Add it to index.html /
// App.razor:
//
//   <script src="_content/CymruBlazor/js/theme.js"></script>
//
window.cymruBlazorTheme = (() => {
  const STORAGE_KEY = "cymru-blazor-theme";

  let dotNetRef = null;

  function getStoredTheme() {
    try {
      return window.localStorage.getItem(STORAGE_KEY);
    } catch {
      // localStorage can throw in some private-browsing modes.
      return null;
    }
  }

  function setStoredTheme(cssTheme) {
    try {
      window.localStorage.setItem(STORAGE_KEY, cssTheme);
    } catch {
      // Best-effort only - see ThemeService.PersistThemeAsync.
    }
  }

  function getPreferredScheme() {
    if (!window.matchMedia) {
      return "light";
    }

    return window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  }

  // Notifies .NET when the OS preference changes live (e.g. the user
  // switches their OS from light to dark mode while the app is open),
  // but only while the app hasn't got an explicit stored preference of
  // its own - an explicit in-app choice should win.
  function watchSystemPreference(dotNetHelper) {
    dotNetRef = dotNetHelper;

    if (!window.matchMedia) {
      return;
    }

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

    mediaQuery.addEventListener("change", (event) => {
      if (getStoredTheme()) {
        // The user has an explicit preference saved - don't override it.
        return;
      }

      dotNetRef?.invokeMethodAsync(
        "OnSystemPreferenceChanged",
        event.matches ? "dark" : "light"
      );
    });
  }

  function disposeWatch() {
    dotNetRef = null;
  }

  return {
    getStoredTheme,
    setStoredTheme,
    getPreferredScheme,
    watchSystemPreference,
    disposeWatch,
  };
})();
