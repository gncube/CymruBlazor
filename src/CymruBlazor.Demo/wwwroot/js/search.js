// Global keyboard shortcut (Ctrl+K / Cmd+K) to open the demo's search modal.
// Mirrors the library's own theme.js pattern: a tiny, framework-free script
// that just bridges a DOM event to a .NET method via DotNetObjectReference.
window.cymruDemoSearch = {
    _handler: null,

    addShortcutListener: function (dotNetRef) {
        this.removeShortcutListener();

        this._handler = function (event) {
            const isShortcut = (event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k';

            if (isShortcut) {
                event.preventDefault();
                dotNetRef.invokeMethodAsync('OnGlobalShortcut');
            }
        };

        document.addEventListener('keydown', this._handler);
    },

    removeShortcutListener: function () {
        if (this._handler) {
            document.removeEventListener('keydown', this._handler);
            this._handler = null;
        }
    },

    focusElement: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.focus();
        }
    }
};
