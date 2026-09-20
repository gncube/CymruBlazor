// CymruBlazor overlay interop (ES module, loaded on demand).
//
// Loaded lazily by the library itself with
//   import("./_content/CymruBlazor/js/cymru-overlay.js")
// so consumers do NOT need a <script> tag for it (unlike cymrublazor.js, the
// theme script). Kept deliberately small: it does only what Blazor cannot do
// without the DOM - move focus, contain Tab, and drive the native <dialog>.
//
// Three groups of exports:
//   1. focus helpers      - focusElement / focusTarget / restoreFocus
//   2. focus trap         - activateTrap / releaseTrap   (CyFocusTrap, CySidebar)
//   3. modal dialog       - showDialog / updateDialog / disposeDialog (CyDialog)
//   4. tooltips           - installTooltips                          (CyTooltip)

const TABBABLE = [
  "a[href]",
  "area[href]",
  "button:not([disabled])",
  "input:not([disabled]):not([type='hidden'])",
  "select:not([disabled])",
  "textarea:not([disabled])",
  "summary",
  "iframe",
  "audio[controls]",
  "video[controls]",
  "[contenteditable]:not([contenteditable='false'])",
  "[tabindex]",
].join(",");

let nextToken = 1;
const restoreStack = [];
const traps = new Map();
const dialogs = new Map();

function isVisible(el) {
  if (!el.isConnected) return false;
  if (el.closest("[inert]")) return false;
  const style = getComputedStyle(el);
  if (style.visibility === "hidden" || style.display === "none") return false;
  return el.getClientRects().length > 0;
}

/** All keyboard-tabbable, visible descendants of `root`, in DOM order. */
export function getTabbable(root) {
  return Array.from(root.querySelectorAll(TABBABLE)).filter((el) => {
    const tabindex = el.getAttribute("tabindex");
    if (tabindex !== null && Number(tabindex) < 0) return false;
    if (el.disabled) return false;
    return isVisible(el);
  });
}

function safeFocus(el, preventScroll) {
  if (!el || !el.isConnected || typeof el.focus !== "function") return false;
  el.focus({ preventScroll: preventScroll !== false });
  return document.activeElement === el;
}

function rememberActive() {
  const active = document.activeElement;
  return active && active !== document.body && active !== document.documentElement
    ? active
    : null;
}

// ---------------------------------------------------------------- 1. helpers

/** Focuses the element with `id`; falls back to its first tabbable descendant. */
export function focusElement(id, preventScroll, remember) {
  const el = document.getElementById(id);
  if (!el) return false;

  const previous = remember ? rememberActive() : null;

  let focused = safeFocus(el, preventScroll);
  if (!focused) {
    const [first] = getTabbable(el);
    focused = safeFocus(first, preventScroll);
  }

  if (focused && previous && previous !== el) restoreStack.push(previous);
  return focused;
}

function activeContainer() {
  const last = Array.from(traps.values()).pop();
  return last ? last.container : document.body;
}

/** target: "First" | "Last" | "Next" | "Previous" | "Current" */
export function focusTarget(target, preventScroll) {
  const tabbable = getTabbable(activeContainer());
  if (tabbable.length === 0) return false;

  const index = tabbable.indexOf(document.activeElement);
  switch (target) {
    case "First":
      return safeFocus(tabbable[0], preventScroll);
    case "Last":
      return safeFocus(tabbable[tabbable.length - 1], preventScroll);
    case "Next":
      return safeFocus(tabbable[(index + 1) % tabbable.length], preventScroll);
    case "Previous":
      return safeFocus(tabbable[(index <= 0 ? tabbable.length : index) - 1], preventScroll);
    default:
      return safeFocus(document.activeElement, preventScroll);
  }
}

/** Returns focus to the element remembered by the last remembering focusElement call. */
export function restoreFocus(preventScroll) {
  while (restoreStack.length > 0) {
    const el = restoreStack.pop();
    if (el.isConnected && safeFocus(el, preventScroll)) return true;
  }
  return false;
}

// ------------------------------------------------------------ 2. focus trap

/**
 * Contains Tab/Shift+Tab inside `container` and (optionally) moves focus in and
 * restores it on release. options: { autoFocus, restoreFocus, preventScroll,
 * mediaQuery }. Returns a token for releaseTrap, or 0 if the media query gate
 * did not match. Only the most recently activated trap acts,
 * so traps can nest.
 */
export function activateTrap(container, options) {
  const opts = { autoFocus: true, restoreFocus: true, preventScroll: true, ...options };

  // Optional gate, e.g. the sidebar drawer only traps on small screens.
  if (opts.mediaQuery && !window.matchMedia(opts.mediaQuery).matches) return 0;

  const token = nextToken++;
  const previous = rememberActive();

  const trap = { container, previous, opts };

  const isTop = () => Array.from(traps.keys()).pop() === token;

  trap.onKeyDown = (event) => {
    if (event.key !== "Tab" || !isTop()) return;

    const tabbable = getTabbable(container);
    if (tabbable.length === 0) {
      event.preventDefault();
      safeFocus(container, opts.preventScroll);
      return;
    }

    const first = tabbable[0];
    const last = tabbable[tabbable.length - 1];
    const active = document.activeElement;
    const outside = !container.contains(active) || active === container;

    if (event.shiftKey && (active === first || outside)) {
      event.preventDefault();
      safeFocus(last, opts.preventScroll);
    } else if (!event.shiftKey && (active === last || outside)) {
      event.preventDefault();
      safeFocus(first, opts.preventScroll);
    }
  };

  // Catches focus leaving by other routes (mouse click outside, script, browser UI).
  trap.onFocusIn = (event) => {
    if (!isTop() || container.contains(event.target)) return;
    const [first] = getTabbable(container);
    safeFocus(first ?? container, opts.preventScroll);
  };

  document.addEventListener("keydown", trap.onKeyDown, true);
  document.addEventListener("focusin", trap.onFocusIn, true);
  traps.set(token, trap);

  if (opts.autoFocus) {
    const preferred = container.querySelector("[autofocus]");
    const [first] = getTabbable(container);
    if (!safeFocus(preferred, opts.preventScroll) && !safeFocus(first, opts.preventScroll)) {
      safeFocus(container, opts.preventScroll);
    }
  }

  return token;
}

/** As activateTrap, looking the container up by id. Returns 0 when it does not exist. */
export function activateTrapById(id, options) {
  const container = document.getElementById(id);
  return container ? activateTrap(container, options) : 0;
}

export function releaseTrap(token) {
  const trap = traps.get(token);
  if (!trap) return;

  document.removeEventListener("keydown", trap.onKeyDown, true);
  document.removeEventListener("focusin", trap.onFocusIn, true);
  traps.delete(token);

  if (trap.opts.restoreFocus) safeFocus(trap.previous, trap.opts.preventScroll);
}

// --------------------------------------------------------------- 3. dialogs

/**
 * Opens `dialog` with showModal() (native focus containment, inert background,
 * Escape and top layer). options: { dismissible, closeOnBackdropClick }.
 * `dotNetRef.OnNativeClosed()` is invoked whenever the dialog closes, whatever
 * the cause. Returns a token for updateDialog/disposeDialog.
 */
export function showDialog(dialog, dotNetRef, options) {
  const token = nextToken++;
  const state = {
    dialog,
    dotNetRef,
    previous: rememberActive(),
    options: { dismissible: true, closeOnBackdropClick: false, ...options },
  };

  state.onCancel = (event) => {
    // Escape. Non-dismissible dialogs must be closed by their own actions.
    if (!state.options.dismissible) event.preventDefault();
  };

  state.onClick = (event) => {
    // The panel fills the dialog, so a click whose target is the <dialog>
    // itself landed on the ::backdrop.
    if (event.target === dialog && state.options.dismissible && state.options.closeOnBackdropClick) {
      dialog.close();
    }
  };

  state.onClose = () => {
    if (state.closed) return;
    state.closed = true;
    restoreAfterClose(state);
    state.dotNetRef?.invokeMethodAsync("OnNativeClosed").catch(() => {});
  };

  dialog.addEventListener("cancel", state.onCancel);
  dialog.addEventListener("click", state.onClick);
  dialog.addEventListener("close", state.onClose);
  dialogs.set(token, state);

  if (!dialog.open) dialog.showModal();
  return token;
}

export function updateDialog(token, options) {
  const state = dialogs.get(token);
  if (state) state.options = { ...state.options, ...options };
}

/** Closes the dialog (if open), removes its listeners and restores focus. Safe to call twice. */
export function disposeDialog(token) {
  const state = dialogs.get(token);
  if (!state) return;

  state.dialog.removeEventListener("cancel", state.onCancel);
  state.dialog.removeEventListener("click", state.onClick);
  state.dialog.removeEventListener("close", state.onClose);
  dialogs.delete(token);

  if (state.dialog.open) state.dialog.close();
  if (!state.closed) restoreAfterClose(state);
}

function restoreAfterClose(state) {
  // Browsers restore focus on close(), but not reliably (Safari, removed
  // elements), so do it explicitly when focus was left on <body>.
  const active = document.activeElement;
  if (!active || active === document.body || state.dialog.contains(active)) {
    safeFocus(state.previous, true);
  }
}

// -------------------------------------------------------------- 4. tooltips

let tooltipsInstalled = false;

/**
 * One delegated pair of listeners for every CyTooltip on the page (idempotent).
 * Showing/hiding is pure CSS (:hover / :focus-within); this only adds WCAG 1.4.13
 * "dismissible": Escape hides the visible tooltip(s) without moving the pointer
 * or focus, and the tooltip comes back on the next hover/focus.
 */
export function installTooltips() {
  if (tooltipsInstalled) return;
  tooltipsInstalled = true;

  document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") return;

    const visible = document.querySelectorAll(
      ".cy-tooltip:hover:not([data-dismissed]), .cy-tooltip:focus-within:not([data-dismissed])");
    if (visible.length === 0) return;

    visible.forEach((tip) => tip.setAttribute("data-dismissed", ""));
    // Escape closed a tooltip; do not also let it close a surrounding dialog.
    event.preventDefault();
  });

  // Re-arm on a fresh interaction (entering the tooltip root, or focus arriving from outside).
  document.addEventListener("mouseenter", (event) => {
    if (event.target instanceof Element && event.target.matches(".cy-tooltip")) {
      event.target.removeAttribute("data-dismissed");
    }
  }, true);

  document.addEventListener("focusin", (event) => {
    const tip = event.target instanceof Element ? event.target.closest(".cy-tooltip") : null;
    if (tip && !(event.relatedTarget instanceof Node && tip.contains(event.relatedTarget))) {
      tip.removeAttribute("data-dismissed");
    }
  });
}
