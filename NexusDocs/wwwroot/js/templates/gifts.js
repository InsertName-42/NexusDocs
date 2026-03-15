document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById('gift-container');
    console.log("Gift container found:", container);
    if (!container) return;

    const isExpired = container.dataset.expired === "true";
    const pageId = container.dataset.pageId;

    //Already checked
    const checkedKeys = JSON.parse(container.dataset.checkedKeys || "[]");
    const listItems = container.querySelectorAll('.doc-body li');

    listItems.forEach((li, index) => {
        const key = `gift-item-${index}`;
        const isAlreadyChecked = checkedKeys.includes(key);
        li.style.cursor = isExpired ? "default" : "pointer";

        //Checkbox UI
        const checkbox = document.createElement('input');
        checkbox.type = "checkbox";
        checkbox.className = "me-2 gift-checkbox";
        checkbox.disabled = isExpired;

        checkbox.checked = isAlreadyChecked;

        li.prepend(checkbox);

        //Interaction logic
        if (!isExpired) {
            li.addEventListener('click', async (e) => {
                if (e.target.type === 'checkbox') {
                    await saveInteraction(pageId, key, checkbox.checked);
                    return;
                }
                checkbox.checked = !checkbox.checked;
                await saveInteraction(pageId, key, checkbox.checked);
                });
        }
    });
    async function saveInteraction(pageId, elementKey, isChecked) {
        try {
            const response = await fetch('/Interactions/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    pageId: parseInt(pageId),
                    elementKey: elementKey,
                    status: isChecked
                })
            });

            if (!response.ok) {
                console.error('Failed to save interaction');
            }
        } catch (error) {
            console.error('Error communicating with server:', error);
        }
    }
});
