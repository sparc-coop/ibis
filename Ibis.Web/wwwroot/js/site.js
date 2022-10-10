//File download
async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(url, fileName) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

//Play message audio
function playAudio(url) {
    console.log('playing ' + url);
    const sound = new Howl({
        src: [url]
    });
    sound.play();
}

function speak(audio) {
    const contentType = "audio/mp3";
    const sound = new Howl({
        src: [`data:${contentType};base64,${audio}`]
    });
    sound.play();
}

window.scrollToBottom = (id) => {
    var div = document.getElementById(id);
    div.scrollTop = div.scrollHeight;
};