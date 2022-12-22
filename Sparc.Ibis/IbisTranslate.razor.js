let translationCache = {};
let dotNet = {};
let app = {};
let observer = {};

//Play message audio
let playAudio = function (url) {
    console.log('playing ' + url);
    const sound = new Howl({
        src: [url]
    });
    sound.play();
}

let ibisIgnoreFilter = function (node) {
    if (node.parentNode.nodeName == 'SCRIPT' || node.ibisTranslated)
        return NodeFilter.FILTER_SKIP;

    if (!node.textContent.trim())
        return NodeFilter.FILTER_SKIP;

    var closest = node.parentElement.closest('.ibis-ignore');
    if (closest)
        return NodeFilter.FILTER_SKIP;

    return NodeFilter.FILTER_ACCEPT;
}

function registerTextNodesUnder(el) {
    var n, walk = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, ibisIgnoreFilter);
    while (n = walk.nextNode())
        registerTextNode(n);
}

function replaceWithTranslatedText() {
    observer.disconnect();
    
    for (let key in translationCache) {
        var translation = translationCache[key];

        if (!translation.translation)
            continue;

        for (let node of translation.nodes) {
            if (node.textContent != translation.translation) {
                node.textContent = translation.translation;
                node.ibisTranslated = true;
            }
            node.parentElement?.classList.remove('ibis-translating');
        }
    }

    observer.observe(app, { childList: true, characterData: true, subtree: true });
}

function registerTextNode(node) {
    var nodeText = node.textContent.trim();
    if (!nodeText || node.ibisRegistered || node.ibisTranslated)
        return;

    node.ibisRegistered = true;
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

function observeCallback(mutations) {
    mutations.forEach(function (mutation) {
        if (mutation.target.classList?.contains('ibis-ignore') || mutation.parentElement?.closest('ibis-ignore'))
            return;

        if (mutation.type == 'characterData') {
            registerTextNode(mutation.target);
        }
        else
            mutation.addedNodes.forEach(registerTextNodesUnder);
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
    app = document.getElementById(targetElementId);
    registerTextNodesUnder(app);
    callIbis();

    observer = new MutationObserver(observeCallback);
    observer.observe(app, { childList: true, characterData: true, subtree: true });
}

function getBrowserLanguage() {
    var lang = (navigator.languages && navigator.languages.length) ? navigator.languages[0] :
        navigator.userLanguage || navigator.language || navigator.browserLanguage || 'en';
    return lang.substring(0, 2);
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

export { init, replaceWithTranslatedText, getBrowserLanguage, playAudio };