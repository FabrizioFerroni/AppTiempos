window.modalHelpers = {
    // hace click en el elemento (acepta ElementReference o id)
    clickElement: function(el) {
        try {
            if (!el) return;
            // si es string (id) buscá por id
            if (typeof el === "string") {
                el = document.getElementById(el);
            }
            el?.click();
        } catch (e) {
            console.error(e);
        }
    },

    // usa la API de bootstrap para ocultar el modal por el id del elemento modal (no del botón)
    hideById: function(modalId) {
        var el = document.getElementById(modalId);
        if (!el) return;
        try {
            // bootstrap 5
            var inst = bootstrap.Modal.getInstance(el);
            if (!inst) inst = new bootstrap.Modal(el);
            inst.hide();
        } catch (e) {
            // fallback: intentamos hacer click al botón de cierre
            var btn = document.querySelector(`#${modalId} [data-bs-dismiss="modal"], #${modalId} .btn-close`);
            if (btn) btn.click();
        }
    },

    // ayuda de debug: devuelve si existe el elemento
    elementExists: function(id) {
        return !!document.getElementById(id);
    }
};

window.modalHelpersSidebar = {
    showModal: function (modalId, guid) {
        try {
            /*const button = document.querySelector(
                `[command="show-modal"][commandfor="${modalId}-${guid}"]`
            );*/
            const button = document.getElementById(`${modalId}-${guid}`);
            if (button) {
                button.click();
                console.log(`Modal ${modalId}-${guid} abierto correctamente.`);
            } else {
                console.warn(`No se encontró el botón para el modal ${modalId}-${guid}.`);
            }
        } catch (err) {
            console.error("Error al abrir el modal:", err);
        }
    },
    showModalBTN: function (modalId) {      
        
        try {
            const button = document.getElementById(`${modalId}`);
            if (button) {
                button.click();
                console.log(`Modal ${modalId} abierto correctamente.`);
            } else {
                console.warn(`No se encontró el botón para el modal ${modalId}.`);
            }
        } catch(err) {
            console.error("Error al abrir el modal:", err);
        }
    },
    closeModal: function (modalId, guid) {
        try {
            /*const button = document.querySelector(
                `[command="close"][commandfor="${modalId}-${guid}"]`
            );*/
            const button = document.getElementById(`${modalId}-${guid}`);
            if (button) {
                button.click();
                console.log(`Modal ${modalId}-${guid} cerrado correctamente.`);
            } else {
                console.warn(`No se encontró el botón para cerrar el modal ${modalId}-${guid}.`);
            }
        } catch (err) {
            console.error("Error al cerrar el modal:", err);
        }
    }
};
