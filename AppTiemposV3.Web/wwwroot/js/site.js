window.checkModalOpen = (commandFor) => {
    const modal = document.querySelector(`[commandfor="${commandFor}"]`);
    return modal?.classList.contains("open") || false;
};

window.openModal = (id) => {
    const modal = document.getElementById(id);
    if (modal) {
        modal.showModal ? modal.showModal() : modal.classList.add("open");
    }
};

window.registerThemeChangeHandler = (dotNetHelper) => {
    const observer = new MutationObserver(() => {
        dotNetHelper.invokeMethodAsync('OnThemeChanged');
    });

    const html = document.documentElement;
    observer.observe(html, { attributes: true, attributeFilter: ['class'] });
};