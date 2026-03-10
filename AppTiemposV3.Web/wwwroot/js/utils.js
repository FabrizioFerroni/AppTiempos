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

window.downloadFileFromStream = (fileName, contentType, base64String) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64String}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    console.log("✅ Archivo descargado:", fileName);
};

window.downloadFileAttachment = (fileName, base64String) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `${base64String}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    console.log("✅ Archivo descargado:", fileName);
};


async function downloadFileFromStreamSQL(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();

    URL.revokeObjectURL(url);
    console.log("✅ Archivo descargado SQL:", fileName);
}

window.restoreBackup = async (id) => {
    const element = document.getElementById(id);
    if (!element) {
        console.warn('restoreBackup: element not found', id);
        return;
    }

    element.click();

    try {
        console.log("✅ Elemento clickeado:", file);
    } catch (error) {
        console.error('Error restoring backup:', error);
    }
}

window.downloadFileFromUrl = async (fileName, url) => {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();

    try {
        console.log("✅ Plantilla descargada:", file);
    } catch (error) {
        console.error('Error Plantilla descargada:', error);
    }
}
