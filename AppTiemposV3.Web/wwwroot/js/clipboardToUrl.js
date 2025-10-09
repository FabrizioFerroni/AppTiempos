window.crmClipboard = {
    enablePasteHandler: (inputId, dotNetHelper) => {
        const input = document.getElementById(inputId);
        if (!input) return;

        input.addEventListener("paste", async (e) => {
            e.preventDefault();

            const typeHtml = "text/html";
            const typePlain = "text/plain";

            let pastedHref = "";
            let pastedText = "";

            const items = await navigator.clipboard.read();

            for (const item of items) {
                const types = item.types;
                if (types.includes(typeHtml)) {
                    const blob = await item.getType(typeHtml);
                    const html = await blob.text();

                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, typeHtml);
                    
                    pastedHref = doc.querySelector("a")?.href || "";
                    pastedText = doc.querySelector("a")?.textContent || "";
                    
                } else if(types.includes(typePlain)){
                    const typeItem = await item.getType(typePlain);
                    pastedHref = await typeItem.text();
                }
            }
            
            if (pastedHref) {
                input.value = pastedHref;

                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync("OnPasteUrlAndText", pastedHref, pastedText);
                }
            }
        });
    }
};
