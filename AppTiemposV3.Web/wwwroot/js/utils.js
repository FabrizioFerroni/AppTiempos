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

window.fixChartScale = (canvasId) => {
    const chart = Chart.instances
        ? Object.values(Chart.instances).find(c => c.canvas.id === canvasId)
        : null;

    if (!chart) {
        return;
    }

    const yScale = chart.scales['y'] || chart.scales['y-axis-0'];
    if (yScale) {
        chart.options.scales.yAxes[0].afterBuildTicks = function(scale) {
            scale.ticks = scale.ticks.filter(t => t >= 0);
            scale.options.ticks.min = 0;
        };
        chart.update();
    }
    
    chart.update();
    console.log("✅ Escala ajustada para:", canvasId);
};