
function textDisplay() {
    // array with texts to type in typewriter
    var dataText = ["Podcast", "Audio", "Video"];

    // type one text in the typwriter
    // keeps calling itself until the text is finished
    function typeWriter(text, i, fnCallback) {
        // check if text isn't finished yet
        if (i < (text.length)) {
            // add next character to h1
            document.getElementById("displaytext").innerHTML = text.substring(0, i + 1) + '<span aria-hidden="true" class="blink">|</span>';

            // wait for a while and call this function again for next character
            setTimeout(function () {
                typeWriter(text, i + 1, fnCallback)
            }, 200);
        }
        // text finished, call callback if there is a callback function
        else if (typeof fnCallback == 'function') {
            // call callback after timeout
            setTimeout(fnCallback, 840);
        }
    }
    // start a typewriter animation for a text in the dataText array
    function StartTextAnimation(i) {
        if (typeof dataText[i] == 'undefined') {
            setTimeout(function () {
                StartTextAnimation(0);
            }, 5000);
        }

        // check if dataText[i] exists
        if (dataText[i] != undefined) {
            if (i < dataText[i].length) {

                // text exists! start typewriter animation
                typeWriter(dataText[i], 0, function () {
                    // after callback (and whole text has been animated), start next text
                    StartTextAnimation(i + 1);
                });
            }
        }
    }
    // start the text animation
    StartTextAnimation(0);
};

//File download
async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

//Play message audio
function playAudio(id) {
    document.getElementById("play-" + id).play();
}

function toggleRoomDropdown(roomId) {
    const dropdown = document.getElementById("dropdown-" + roomId);
    if (dropdown.style.visibility == "visible") {
        dropdown.style.visibility = "hidden";
    } else {
        dropdown.style.visibility = "visible";
    }
}

function copyToClipboard(id, copyType) {
    var copyText = document.getElementById(id).value;
    navigator.clipboard.writeText(copyText);

    if (copyType == "list") {
        document.getElementById("copy-" + id).textContent += "Copied!";
        setTimeout(function () {
            document.getElementById("copy-" + id).textContent = "";
        }, 2000);
    }
}