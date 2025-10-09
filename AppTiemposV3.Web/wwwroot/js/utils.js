window.clickElementById = (id) => {
    const el = document.getElementById(id);
    if (el) el.click();
};


window.dialogHelpers = {
    showDialogById: function (id) {
        try {
            const el = document.getElementById(id);
            if (!el) { console.warn('dialogHelpers: element not found', id); return; }
            if (typeof el.showModal === 'function') {
                el.showModal();
            } else {
                // fallback: trigger click
                el.click();
            }
        } catch (e) {
            console.error('dialogHelpers.showDialogById error', e);
        }
    },
    closeDialogById: function (id) {
        try {
            const el = document.getElementById(id);
            if (!el) return;
            if (typeof el.close === 'function') {
                el.close();
            } else {
                el.click();
            }
        } catch (e) {
            console.error('dialogHelpers.closeDialogById error', e);
        }
    }
};


window.getInputValue = function (el) {
    return el.value;
};

window.setInputValue = function (el, val) {
    el.value = val;
};


window.clipboardHelper = {
    getText: async () => {
        return await navigator.clipboard.readText();
    }
};

window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error("Error al copiar: ", err);
        return false;
    }
};