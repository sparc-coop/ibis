let translationCache = {};
let dotNet = {};
let app = {};
let observer = {};
let language = getBrowserLanguage();

let ibisIgnoreFilter = function (node) {
    if (node.parentNode.nodeName == 'SCRIPT' || node.ibisTranslated == language)
        return NodeFilter.FILTER_SKIP;

    if (!node.textContent.trim())
        return NodeFilter.FILTER_SKIP;

    var closest = node.parentElement.closest('.ibis-ignore');
    if (closest)
        return NodeFilter.FILTER_SKIP;

    return NodeFilter.FILTER_ACCEPT;
}

function init(targetElementId, selectedLanguage, dotNetObjectReference, serverTranslationCache) {
    language = selectedLanguage;
    dotNet = dotNetObjectReference;
    
    if (serverTranslationCache)
        translationCache = serverTranslationCache;
    else {
        for (let key in translationCache) {
            translationCache[key].submitted = false;
            translationCache[key].translation = null;
        }
    }

    if (/complete|interactive|loaded/.test(document.readyState)) {
        initElement(targetElementId);
    } else {
        window.addEventListener('DOMContentLoaded', () => initElement(targetElementId));
    }
}

function initElement(targetElementId) {
    app = document.getElementById(targetElementId);
    registerTextNodesUnder(app);
    callIbis();

    observer = new MutationObserver(observeCallback);
    observer.observe(app, { childList: true, characterData: true, subtree: true });
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

function registerTextNodesUnder(el) {
    var n, walk = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, ibisIgnoreFilter);
    while (n = walk.nextNode())
        registerTextNode(n);
}

function registerTextNode(node) {
    if (node.ibisRegistered == language || node.ibisTranslated == language)
        return;

    var nodeText = node.ibisContent ?? node.textContent.trim();
    if (!nodeText)
        return;

    node.ibisRegistered = language;
    node.ibisContent = nodeText;
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

function replaceWithTranslatedText() {
    observer.disconnect();

    for (let key in translationCache) {
        var translation = translationCache[key];

        if (!translation.translation)
            continue;

        for (let node of translation.nodes) {
            if (node.textContent != translation.translation) {
                node.textContent = translation.translation;
                node.ibisTranslated = language;
            }
            node.parentElement?.classList.remove('ibis-translating');
        }
    }

    observer.observe(app, { childList: true, characterData: true, subtree: true });
}


function getBrowserLanguage() {
    var lang = (navigator.languages && navigator.languages.length) ? navigator.languages[0] :
        navigator.userLanguage || navigator.language || navigator.browserLanguage || 'en';
    return lang.substring(0, 2);
}

let playAudio = function (url) {
    const sound = new Howl({
        src: [url]
    });
    sound.play();
}

export { init, replaceWithTranslatedText, getBrowserLanguage, playAudio };