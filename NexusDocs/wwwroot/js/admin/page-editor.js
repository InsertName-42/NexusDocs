document.addEventListener("DOMContentLoaded", function () {
    const templateSelector = document.getElementById('templateSelector');
    const eventDateSection = document.getElementById('eventDateSection');

    if (templateSelector && eventDateSection) {
        const toggleDate = () => {
            const selectedText = templateSelector.options[templateSelector.selectedIndex].text;
            eventDateSection.style.display = (selectedText === "Gifts") ? "block" : "none";
        };

        templateSelector.addEventListener('change', toggleDate);
        toggleDate();
    }
});