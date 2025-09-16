window.sidebarInterop = {
    getSidebarCookie: function (name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    setSidebarCookie: function (name, value, maxAge) {
        document.cookie = `${name}=${value}; path=/; max-age=${maxAge}`;
    },
    isMobile: function () {
        return window.matchMedia("(max-width: 768px)").matches;
    },
    addKeyDownListener: function (dotNetHelper, key, ctrlOrMetaKey, methodName) {
        const handler = (event) => {
            if (event.key === key && (ctrlOrMetaKey ? (event.metaKey || event.ctrlKey) : true)) {
                event.preventDefault();
                dotNetHelper.invokeMethodAsync(methodName);
            }
        };
        window.addEventListener("keydown", handler);
        return handler; // Return handler to be able to remove it later
    },
    removeKeyDownListener: function (handler) {
        window.removeEventListener("keydown", handler);
    }
};