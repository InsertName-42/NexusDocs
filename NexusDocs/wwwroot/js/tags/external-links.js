(function () {
    const initExternalLinks = () => {
        const contentBody = document.querySelector('.doc-body');
        if (!contentBody) return;

        //Convert plain text URLs into <a> tags
        const urlRegex = /(https?:\/\/[^\s<]+)/g;

        const walk = document.createTreeWalker(contentBody, NodeFilter.SHOW_TEXT, null, false);
        let node;
        const textNodes = [];
        while (node = walk.nextNode()) textNodes.push(node);

        textNodes.forEach(textNode => {
            if (urlRegex.test(textNode.nodeValue)) {
                const span = document.createElement('span');
                span.innerHTML = textNode.nodeValue.replace(urlRegex, '<a href="$1">$1</a>');
                textNode.parentNode.replaceChild(span, textNode);
            }
        });

        const links = contentBody.querySelectorAll('a');
        const currentHost = window.location.hostname;

        links.forEach(link => {
            try {
                const url = new URL(link.href);

                if (url.hostname !== currentHost && url.protocol.startsWith('http')) {
                    link.setAttribute('target', '_blank');
                    link.setAttribute('rel', 'noopener noreferrer');

                    if (!link.querySelector('.bi-box-arrow-up-right')) {
                        const icon = document.createElement('i');
                        icon.className = "bi bi-box-arrow-up-right ms-1 small opacity-75";
                        link.appendChild(icon);
                    }
                }
            } catch (e) {
            }
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initExternalLinks);
    } else {
        initExternalLinks();
    }
})();