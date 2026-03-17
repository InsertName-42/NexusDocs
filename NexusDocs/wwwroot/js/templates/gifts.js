document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById('gift-container');
    if (!container) return;

    const isExpired = container.dataset.expired === "true";
    const pageId = container.dataset.pageId;

    //Already checked
    const checkedKeys = JSON.parse(container.dataset.checkedKeys || "[]");
    const listItems = container.querySelectorAll('.doc-body li');

    listItems.forEach((li, index) => {
        //Create a key from the text 
        const cleanText = li.innerText.trim().toLowerCase().substring(0, 80);
        const key = cleanText.replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');

        if (!key) return;
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
                    //Send update if the checkbox was clicked
                    await saveInteraction(pageId, key, checkbox.checked);
                    return;
                }

                checkbox.checked = !checkbox.checked;
                await saveInteraction(pageId, key, checkbox.checked);
            });
        }
    });

    //Helper function to handle the fetch request to the InteractionsController
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