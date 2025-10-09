window.selectHelper = (function () {
    let dotnetRef = null;
    let elementId = null;

    function handleKeydown(e) {
        if (e.key === "Escape" && dotnetRef) {
            dotnetRef.invokeMethodAsync("OnEscapePressed");
        }
    }

    function handleClick(e) {
        if (!dotnetRef || !elementId) return;
        const el = document.getElementById(elementId);
        if (!el) return;

        if (!el.contains(e.target)) {
            dotnetRef.invokeMethodAsync("OnOutsideClick");
        }
    }

    return {
        register: function (ref, id) {
            dotnetRef = ref;
            elementId = id;

            window.addEventListener("keydown", handleKeydown);
            window.addEventListener("click", handleClick, true); // << capture phase
        },
        unregister: function () {
            window.removeEventListener("keydown", handleKeydown);
            window.removeEventListener("click", handleClick, true);
            dotnetRef = null;
            elementId = null;
        },
    };
})();

