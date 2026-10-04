// CymruBlazor sortable list interop (ES module, loaded on demand by CySortableList).
//
// Pointer-driven drag and drop with NO third-party dependency. Keyboard and
// single-pointer reordering live in C# (CySortableList); this module only does
// what needs the DOM:
//
//   * register / update / dispose  - pointer dragging within and between lists
//     that share a group (Pointer Events + setPointerCapture, so mouse, touch
//     and pen behave the same; handles set `touch-action: none` in CSS)
//   * keyboard guard               - stops arrows / Home / End / Space scrolling
//     the page while a handle is "picked up" (data-cy-picked="true")
//   * focusIn                      - re-focuses a control after Blazor re-orders the DOM
//
// DOM contract (rendered by CySortableList):
//   ul[data-cy-sortable][data-cy-list-id][data-cy-group]
//     li[data-cy-item][data-cy-index]
//       button[data-cy-drag-handle]
//
// On drop the module calls   dotNetRef.invokeMethodAsync("OnPointerDrop",
//   targetListId, oldIndex, newIndex)   on the list the drag started in.
// `newIndex` is the item's final index in the target list.

const DRAG_THRESHOLD = 4;     // px before a press becomes a drag
const EDGE = 48;              // px from the viewport edge that starts auto-scroll
const SCROLL_STEP = 14;
const INTERACTIVE = "button, a[href], input, select, textarea, label, [contenteditable], [role='button'], [role='menuitem']";

let nextToken = 1;
const lists = new Map();      // token -> state
const roots = new Map();      // root element -> state

function reduceMotion() {
  return window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function itemsOf(root) {
  return Array.from(root.children).filter((el) => el.hasAttribute("data-cy-item"));
}

// ------------------------------------------------------------- registration

export function register(root, dotNetRef, options) {
  const state = {
    token: nextToken++,
    root,
    dotNetRef,
    options: { group: null, handleOnly: true, disabled: false, ...options },
    drag: null,
  };

  state.onPointerDown = (event) => startPress(state, event);
  state.onKeyDown = (event) => guardKey(root, event);
  state.onKeyUp = (event) => guardKey(root, event);

  root.addEventListener("pointerdown", state.onPointerDown);
  root.addEventListener("keydown", state.onKeyDown);
  root.addEventListener("keyup", state.onKeyUp);

  lists.set(state.token, state);
  roots.set(root, state);
  return state.token;
}

export function update(token, options) {
  const state = lists.get(token);
  if (state) state.options = { ...state.options, ...options };
}

export function dispose(token) {
  const state = lists.get(token);
  if (!state) return;
  if (state.drag) endDrag(state, false);
  state.root.removeEventListener("pointerdown", state.onPointerDown);
  state.root.removeEventListener("keydown", state.onKeyDown);
  state.root.removeEventListener("keyup", state.onKeyUp);
  lists.delete(token);
  roots.delete(state.root);
}

/** Focuses `selector` inside the item at `index` (default: its drag handle). Returns whether focus moved. */
export function focusIn(root, index, selector) {
  const item = itemsOf(root)[index];
  const target = item && item.querySelector(selector || "[data-cy-drag-handle]");
  if (!target || target.disabled) return false;
  target.focus({ preventScroll: false });
  return document.activeElement === target;
}

// ------------------------------------------------------------ keyboard guard

// Escape is guarded too, so cancelling a pick-up inside a drawer does not also close the drawer.
const GUARDED = new Set(["ArrowUp", "ArrowDown", "Home", "End", "Escape"]);

function guardKey(root, event) {
  if (!GUARDED.has(event.key)) return;
  const handle = event.target instanceof Element ? event.target.closest("[data-cy-drag-handle]") : null;
  if (!handle || handle.getAttribute("data-cy-picked") !== "true") return;
  if (!root.contains(handle)) return;
  // Space / Enter are deliberately NOT guarded: they reach .NET as the button's
  // own click (detail 0), which is also how assistive technology activates it.
  event.preventDefault();
}

// ----------------------------------------------------------------- pointer drag

function startPress(state, event) {
  if (state.options.disabled || state.drag) return;
  if (event.pointerType === "mouse" && event.button !== 0) return;
  if (!(event.target instanceof Element)) return;

  let grip = event.target.closest("[data-cy-drag-handle]");
  let item;

  if (grip && state.root.contains(grip)) {
    if (grip.disabled) return;
    item = grip.closest("[data-cy-item]");
  } else if (!state.options.handleOnly) {
    item = event.target.closest("[data-cy-item]");
    if (!item || event.target.closest(INTERACTIVE)) return;
    grip = item;
  } else {
    return;
  }

  if (!item || item.parentElement !== state.root) return;

  state.drag = {
    pointerId: event.pointerId,
    grip,
    item,
    sourceRoot: state.root,
    oldIndex: Number(item.getAttribute("data-cy-index")),
    startX: event.clientX,
    startY: event.clientY,
    x: event.clientX,
    y: event.clientY,
    active: false,
    target: null,       // { root, index }
    ghostOffset: { x: 0, y: 0 },
    suppressClick: false,
  };

  try { grip.setPointerCapture(event.pointerId); } catch { /* element detached */ }

  state.drag.onMove = (e) => onMove(state, e);
  state.drag.onUp = (e) => onUp(state, e, true);
  state.drag.onCancel = (e) => onUp(state, e, false);
  state.drag.onKey = (e) => { if (e.key === "Escape") { e.preventDefault(); endDrag(state, false); } };

  grip.addEventListener("pointermove", state.drag.onMove);
  grip.addEventListener("pointerup", state.drag.onUp);
  grip.addEventListener("pointercancel", state.drag.onCancel);
  grip.addEventListener("lostpointercapture", state.drag.onCancel);
  document.addEventListener("keydown", state.drag.onKey, true);
}

function onMove(state, event) {
  const drag = state.drag;
  if (!drag || event.pointerId !== drag.pointerId) return;
  drag.x = event.clientX;
  drag.y = event.clientY;

  if (!drag.active) {
    if (Math.hypot(drag.x - drag.startX, drag.y - drag.startY) < DRAG_THRESHOLD) return;
    beginDrag(state);
  }

  event.preventDefault();
  positionGhost(drag);
  autoScroll(drag);
  updateTarget(state);
}

function beginDrag(state) {
  const drag = state.drag;
  drag.active = true;
  drag.suppressClick = true;
  state.root.setAttribute("data-cy-dragging", "true");
  drag.item.setAttribute("data-cy-drag-source", "true");
  document.documentElement.setAttribute("data-cy-sorting", "true");

  const rect = drag.item.getBoundingClientRect();
  drag.width = rect.width;
  drag.height = rect.height;
  drag.ghostOffset = { x: drag.startX - rect.left, y: drag.startY - rect.top };

  // A clone follows the pointer so layout of the real list is undisturbed;
  // the original stays in place, dimmed, and marks where the item came from.
  const ghost = drag.item.cloneNode(true);
  ghost.removeAttribute("id");
  ghost.querySelectorAll("[id]").forEach((el) => el.removeAttribute("id"));
  ghost.setAttribute("aria-hidden", "true");
  ghost.setAttribute("inert", "");
  ghost.removeAttribute("data-cy-item");
  ghost.setAttribute("data-cy-ghost", "true");
  ghost.style.cssText =
    `position:fixed;z-index:2147483000;pointer-events:none;margin:0;` +
    `inline-size:${rect.width}px;block-size:${rect.height}px;inset-block-start:0;inset-inline-start:0;order:0;`;
  document.body.appendChild(ghost);
  drag.ghost = ghost;
}

function positionGhost(drag) {
  if (!drag.ghost) return;
  const x = drag.x - drag.ghostOffset.x;
  const y = drag.y - drag.ghostOffset.y;
  drag.ghost.style.transform = `translate(${x}px, ${y}px)`;
}

function autoScroll(drag) {
  if (reduceMotion()) return;
  if (drag.y < EDGE) window.scrollBy(0, -SCROLL_STEP);
  else if (drag.y > window.innerHeight - EDGE) window.scrollBy(0, SCROLL_STEP);
}

/** The sortable list under the pointer that accepts this drag (same group), else null. */
function listUnderPointer(state, x, y) {
  const group = state.options.group;
  const elements = document.elementsFromPoint(x, y);
  for (const el of elements) {
    const root = el.closest && el.closest("[data-cy-sortable]");
    if (!root) continue;
    const other = roots.get(root);
    if (!other || other.options.disabled) continue;
    if (root === state.root) return root;
    if (group && other.options.group === group) return root;
  }
  return null;
}

/** Index in `root` (ignoring the dragged item) where the pointer's y falls. */
function insertionIndex(root, drag) {
  const others = itemsOf(root).filter((el) => el !== drag.item);
  for (let i = 0; i < others.length; i++) {
    const rect = others[i].getBoundingClientRect();
    if (drag.y < rect.top + rect.height / 2) return i;
  }
  return others.length;
}

function clearIndicators() {
  document.querySelectorAll("[data-cy-drop]").forEach((el) => el.removeAttribute("data-cy-drop"));
  document.querySelectorAll("[data-cy-drop-target]").forEach((el) => el.removeAttribute("data-cy-drop-target"));
}

function updateTarget(state) {
  const drag = state.drag;
  clearIndicators();

  const root = listUnderPointer(state, drag.x, drag.y);
  if (!root) {
    drag.target = null;
    return;
  }

  const index = insertionIndex(root, drag);
  drag.target = { root, index };
  root.setAttribute("data-cy-drop-target", "true");

  const others = itemsOf(root).filter((el) => el !== drag.item);
  if (index < others.length) others[index].setAttribute("data-cy-drop", "before");
  else if (others.length > 0) others[others.length - 1].setAttribute("data-cy-drop", "after");
}

function onUp(state, event, commit) {
  const drag = state.drag;
  if (!drag || (event.pointerId !== undefined && event.pointerId !== drag.pointerId)) return;
  endDrag(state, commit && drag.active);
}

function endDrag(state, commit) {
  const drag = state.drag;
  if (!drag) return;
  state.drag = null;

  const { grip, target } = drag;
  grip.removeEventListener("pointermove", drag.onMove);
  grip.removeEventListener("pointerup", drag.onUp);
  grip.removeEventListener("pointercancel", drag.onCancel);
  grip.removeEventListener("lostpointercapture", drag.onCancel);
  document.removeEventListener("keydown", drag.onKey, true);
  try { grip.releasePointerCapture(drag.pointerId); } catch { /* already released */ }

  clearIndicators();
  state.root.removeAttribute("data-cy-dragging");
  drag.item.removeAttribute("data-cy-drag-source");
  document.documentElement.removeAttribute("data-cy-sorting");
  if (drag.ghost) drag.ghost.remove();

  if (drag.suppressClick) {
    // A drag that ends over its own handle would otherwise fire a click.
    const swallow = (e) => { e.stopPropagation(); e.preventDefault(); };
    grip.addEventListener("click", swallow, { capture: true, once: true });
    setTimeout(() => grip.removeEventListener("click", swallow, true), 0);
  }

  if (!commit || !target) return;

  const targetId = target.root.getAttribute("data-cy-list-id");
  const sameList = target.root === state.root;
  if (sameList && target.index === drag.oldIndex) return;   // dropped where it started

  state.dotNetRef.invokeMethodAsync("OnPointerDrop", targetId, drag.oldIndex, target.index).catch(() => {});
}
