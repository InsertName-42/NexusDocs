document.addEventListener("DOMContentLoaded", function () {
    const templateSelector = document.getElementById('templateSelector');
    const eventDateSection = document.getElementById('eventDateSection');
    const giftWarning = document.getElementById('giftTemplateWarning');

    function updateFormVisibility() {
        const selectedTemplate = templateSelector.options[templateSelector.selectedIndex].text;

        if (selectedTemplate === "Gifts") {
            eventDateSection?.classList.remove('d-none');
            giftWarning?.classList.remove('d-none');
        } else {
            eventDateSection?.classList.add('d-none');
            giftWarning?.classList.add('d-none');
        }
    }
    if (templateSelector) {
        updateFormVisibility();

        templateSelector.addEventListener('change', updateFormVisibility);
    }
});