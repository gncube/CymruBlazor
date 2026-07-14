/**
 * CymruBlazor Theme Service - JavaScript Interop
 *
 * Manages DOM manipulation for theme switching.
 * Handles data-theme attribute updates and local storage persistence.
 */

export function getStoredTheme() {
  try {
    return localStorage.getItem("cymru-theme") || "Light";
  } catch {
    return "Light";
  }
}

export function storeTheme(theme) {
  try {
    localStorage.setItem("cymru-theme", theme);
  } catch {
    console.warn("Failed to store theme preference");
  }
}

export function setTheme(themeAttribute) {
  try {
    const htmlElement = document.documentElement;

    if (themeAttribute) {
      htmlElement.setAttribute("data-theme", themeAttribute);
    } else {
      htmlElement.removeAttribute("data-theme");
    }

    // Trigger theme change event for listeners
    window.dispatchEvent(
      new CustomEvent("cymru-theme-changed", {
        detail: { theme: themeAttribute },
      }),
    );
  } catch (error) {
    console.error("Failed to set theme:", error);
  }
}

export function getCurrentTheme() {
  try {
    return document.documentElement.getAttribute("data-theme") || "Light";
  } catch {
    return "Light";
  }
}

export function onThemeChanged(callback) {
  window.addEventListener("cymru-theme-changed", (event) => {
    callback(event.detail.theme);
  });
}
