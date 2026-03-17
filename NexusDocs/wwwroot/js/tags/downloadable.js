(function () {
    const initDownload = () => {
        const downloadBtn = document.getElementById('downloadPdf');
        if (!downloadBtn) return;

        downloadBtn.addEventListener('click', () => {
            const element = document.querySelector('.doc-body');
            if (!element) return;

            const pageTitle = document.querySelector('h1')?.innerText || 'document';

            const options = {
                margin: 1,
                filename: `${pageTitle}.pdf`,
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2 },
                jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
            };

            html2pdf().from(element).set(options).save();
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDownload);
    } else {
        initDownload();
    }
})();