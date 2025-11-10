window.crmClipboard = {
    enablePasteHandler: (inputOrId, dotNetHelper) => {
        const input = typeof inputOrId === 'string'
            ? document.getElementById(inputOrId)
            : inputOrId;

        if (!input) {
            console.warn("⚠️ No se encontró el input");
            return;
        }

        console.log(`✅ Handler conectado a: ${input.id}`);

        input.addEventListener("paste", async (e) => {
            const clipboardData = e.clipboardData || window.clipboardData;
            console.log("📋 Evento paste detectado");

            let pastedHref = "", pastedText = "";

            if (clipboardData) {
                const html = clipboardData.getData("text/html");
                const text = clipboardData.getData("text/plain");

                if (html) {
                    e.preventDefault();
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, "text/html");
                    pastedHref = doc.querySelector("a")?.href || "";
                    pastedText = doc.querySelector("a")?.textContent || "";
                } else if (text) {
                    pastedHref = text;
                    pastedText = text;
                }
            }

            if (pastedHref) {
                input.value = pastedHref;
                input.dispatchEvent(new Event("input", { bubbles: true }));
                console.log(`✅ Pegado en el input id: ${input.id}`);

                if (dotNetHelper)
                    dotNetHelper.invokeMethodAsync("OnPasteUrlAndText", pastedHref, pastedText);
            }
        });
    }
};

