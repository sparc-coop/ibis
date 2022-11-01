let translationCache = {};
let dotNet = {};

function textNodesUnder(el) {
    var n, a = [], walk = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, null, false);
    while (n = walk.nextNode())
        a.push(n);

    var f = a.filter(x => x.parentNode.nodeName != 'SCRIPT' && x.textContent.trim().length);
    return f;
}

function replaceWithTranslatedText() {
    for (let key in translationCache) {
        var translation = translationCache[key];

        if (!translation.translation)
            continue;

        for (let node of translation.nodes) {
            if (node.textContent != translation.translation) {
                node.textContent = translation.translation;
                node.translated = true;
            }
            node.parentElement?.classList.remove('ibis-translating');
        }
    }
}

function registerTextNode(node) {
    var nodeText = node.textContent.trim();
    if (!nodeText || node.translated)
        return;

    node.parentElement?.classList.add('ibis-translating');
    if (nodeText in translationCache && translationCache[nodeText].nodes.indexOf(node) < 0) {
        translationCache[nodeText].nodes.push(node);
    } else {
        translationCache[nodeText] = {
            nodes: [node],
            translation: null
        };
    }
}

function registerDocumentNode(node) {
    var mutationTextNodes = textNodesUnder(node);
    mutationTextNodes.forEach(n => registerTextNode(n));
}

function observeCallback(mutations) {
    mutations.forEach(function (mutation) {
        if (mutation.type == 'characterData') {
            registerTextNode(mutation.target);
        }
        else
            mutation.addedNodes.forEach(registerDocumentNode);
    });

    callIbis();
}

function callIbis() {
    var contentToTranslate = [];
    for (let key in translationCache) {
        if (!translationCache[key].submitted && !translationCache[key].translation) {
            translationCache[key].submitted = true;
            contentToTranslate.push(key);
        }
    }

    dotNet.invokeMethodAsync("TranslateAsync", contentToTranslate).then(translations => {
        for (var i = 0; i < translations.length; i++) {
            translationCache[contentToTranslate[i]].translation = translations[i];
        }

        replaceWithTranslatedText();
    });
}

function observe(targetElementId) {
    var app = document.getElementById(targetElementId);
    registerDocumentNode(app);
    callIbis();

    var observer = new MutationObserver(observeCallback);
    observer.observe(app, { childList: true, characterData: true, subtree: true });
}

function init(targetElementId, dotNetObjectReference, serverTranslationCache) {
    dotNet = dotNetObjectReference;
    if (serverTranslationCache)
        translationCache = serverTranslationCache;

    if (/complete|interactive|loaded/.test(document.readyState)) {
        observe(targetElementId);
    } else {
        window.addEventListener('DOMContentLoaded', () => observe(targetElementId));
    }
}

export { init, replaceWithTranslatedText };