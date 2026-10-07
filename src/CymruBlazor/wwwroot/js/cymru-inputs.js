// CymruBlazor input interop (ES module, loaded on demand by CyCombobox / CyMultiCombobox).
//
// Blazor can only preventDefault on every key of an element or on none, so the few keys whose
// browser default would fight an open combobox list are handled here. All state is read from the
// DOM (aria-expanded, aria-activedescendant): there are no round trips to .NET. Keep this file
// small and dependency free.
//
//   attachCombobox(input)   - prevent the default action of ArrowUp/ArrowDown (caret jumps),
//                             Enter on an active option (would submit the surrounding form) and
//                             Escape when it closes the list or clears the text (would also close
//                             an enclosing dialog or drawer); keep the active option scrolled
//                             into view inside its list.
//   detachCombobox(token)
//
// Events are never stopped from propagating: Blazor's own delegated handler still has to see them.

let nextToken = 1;
const comboboxes = new Map();

export function attachCombobox(input) {
  const onKeyDown = (event) => {
    if (event.defaultPrevented || event.isComposing || event.ctrlKey || event.metaKey) return;

    const expanded = input.getAttribute("aria-expanded") === "true";
    const active = !!input.getAttribute("aria-activedescendant");

    switch (event.key) {
      case "ArrowDown":
      case "ArrowUp":
        event.preventDefault();
        break;
      case "Enter":
        if (expanded && active) event.preventDefault();
        break;
      case "Escape":
        if (expanded || input.value !== "") event.preventDefault();
        break;
      default:
        break;
    }
  };

  const reveal = () => {
    const id = input.getAttribute("aria-activedescendant");
    if (!id) return;
    const option = document.getElementById(id);
    const list = option && option.closest('[role="listbox"]');
    if (!option || !list) return;

    // Scroll the list itself, never the page.
    const o = option.getBoundingClientRect();
    const l = list.getBoundingClientRect();
    if (o.top < l.top) {
      list.scrollTop -= l.top - o.top;
    } else if (o.bottom > l.bottom) {
      list.scrollTop += o.bottom - l.bottom;
    }
  };

  const observer = new MutationObserver(reveal);
  observer.observe(input, { attributes: true, attributeFilter: ["aria-activedescendant"] });
  input.addEventListener("keydown", onKeyDown);

  const token = nextToken++;
  comboboxes.set(token, () => {
    observer.disconnect();
    input.removeEventListener("keydown", onKeyDown);
  });
  return token;
}

export function detachCombobox(token) {
  const dispose = comboboxes.get(token);
  if (dispose) {
    dispose();
    comboboxes.delete(token);
  }
}
