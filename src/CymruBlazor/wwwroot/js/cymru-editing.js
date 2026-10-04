// CymruBlazor editing interop (ES module, loaded on demand by CyMenu / CyToolbar).
//
// Blazor cannot call preventDefault per key, so arrow-key page scrolling and
// the roving tabindex of an arbitrary toolbar need a little DOM help. Keep this
// file small and dependency free.
//
//   attachMenu(root, dotNetRef)      - stop ArrowUp/Down/Home/End scrolling the page and
//                                      report pointer presses outside the menu to .NET
//   detachMenu(token)
//   attachToolbar(el, orientation)   - roving tabindex + arrow/Home/End navigation
//   detachToolbar(token)

let nextToken = 1;
const menus = new Map();
const toolbars = new Map();

// ------------------------------------------------------------------- menu

export function attachMenu(root, dotNetRef) {
  const onKeyDown = (event) => {
    const inList = !!(event.target instanceof Element && event.target.closest('[role="menu"]'));
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();
    } else if ((event.key === "Home" || event.key === "End") && inList) {
      event.preventDefault();
    }
  };

  const onPointerDown = (event) => {
    if (!root.isConnected || root.contains(event.target)) return;
    dotNetRef.invokeMethodAsync("OnOutsidePointer").catch(() => {});
  };

  root.addEventListener("keydown", onKeyDown);
  document.addEventListener("pointerdown", onPointerDown, true);

  const token = nextToken++;
  menus.set(token, () => {
    root.removeEventListener("keydown", onKeyDown);
    document.removeEventListener("pointerdown", onPointerDown, true);
  });
  return token;
}

export function detachMenu(token) {
  const dispose = menus.get(token);
  if (dispose) {
    dispose();
    menus.delete(token);
  }
}

// ---------------------------------------------------------------- toolbar

const ITEM_SELECTOR = [
  "button",
  "a[href]",
  "input:not([type='hidden'])",
  "select",
  "textarea",
  "[role='button']",
  "[role='switch']",
  "[role='checkbox']",
  "[role='radio']",
  "[role='menuitem']",
  "[tabindex]",
].join(",");

function isTextEditable(el) {
  if (!(el instanceof HTMLElement)) return false;
  if (el.isContentEditable) return true;
  if (el instanceof HTMLTextAreaElement || el instanceof HTMLSelectElement) return true;
  if (el instanceof HTMLInputElement) {
    return !["button", "checkbox", "radio", "submit", "reset", "image", "range", "color", "file"].includes(el.type);
  }
  return false;
}

/** The toolbar's focusable controls in DOM order, excluding nested menu popups. */
export function toolbarItems(el) {
  return Array.from(el.querySelectorAll(ITEM_SELECTOR)).filter((item) => {
    if (item.closest('[role="menu"]')) return false;
    if (item.disabled || item.getAttribute("aria-disabled") === "true") return false;
    if (item.hidden || item.closest("[hidden],[inert]")) return false;
    return item.getClientRects().length > 0 || !item.isConnected;
  });
}

export function attachToolbar(el, orientation) {
  const vertical = orientation === "vertical";
  const nextKey = vertical ? "ArrowDown" : "ArrowRight";
  const prevKey = vertical ? "ArrowUp" : "ArrowLeft";

  /** Makes `active` the only tab stop. Defaults to the current stop, else the first item. */
  const sync = (active) => {
    const items = toolbarItems(el);
    if (items.length === 0) return;
    const target =
      (active && items.includes(active) && active) ||
      items.find((item) => item.getAttribute("tabindex") === "0") ||
      items[0];
    for (const item of items) {
      item.setAttribute("tabindex", item === target ? "0" : "-1");
    }
  };

  const onKeyDown = (event) => {
    if (event.defaultPrevented || event.altKey || event.ctrlKey || event.metaKey) return;
    const items = toolbarItems(el);
    const index = items.indexOf(event.target);
    if (index < 0) return;

    let next = -1;
    if (event.key === nextKey || event.key === prevKey) {
      // Left/Right must keep working inside a text field.
      if (isTextEditable(event.target)) return;
      next = (index + (event.key === nextKey ? 1 : -1) + items.length) % items.length;
    } else if (event.key === "Home" && !isTextEditable(event.target)) {
      next = 0;
    } else if (event.key === "End" && !isTextEditable(event.target)) {
      next = items.length - 1;
    } else {
      return;
    }

    event.preventDefault();
    sync(items[next]);
    items[next].focus();
  };

  const onFocusIn = (event) => {
    if (event.target instanceof Element && !event.target.closest('[role="menu"]')) {
      sync(event.target);
    }
  };

  const observer = new MutationObserver(() => sync(document.activeElement));
  observer.observe(el, { childList: true, subtree: true, attributes: true, attributeFilter: ["disabled", "hidden"] });

  el.addEventListener("keydown", onKeyDown);
  el.addEventListener("focusin", onFocusIn);
  sync(null);

  const token = nextToken++;
  toolbars.set(token, () => {
    observer.disconnect();
    el.removeEventListener("keydown", onKeyDown);
    el.removeEventListener("focusin", onFocusIn);
  });
  return token;
}

export function detachToolbar(token) {
  const dispose = toolbars.get(token);
  if (dispose) {
    dispose();
    toolbars.delete(token);
  }
}
