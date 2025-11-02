window.clickOutsideHandler = {
    add: function (dotNetObjRef, elementId) {
        document.addEventListener('click', function (e) {
            const el = document.getElementById(elementId);
            if (el && !el.contains(e.target)) {
                dotNetObjRef.invokeMethodAsync('CloseCalendar');
            }
        });
    }
};