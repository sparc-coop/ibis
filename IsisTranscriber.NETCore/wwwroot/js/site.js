// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

document.addEventListener('DOMContentLoaded', function (event) {
    // array with texts to type in typewriter
    var dataText = ["Podcast", "Audio", "Video"];

    // type one text in the typwriter
    // keeps calling itself until the text is finished
    function typeWriter(text, i, fnCallback) {
        // chekc if text isn't finished yet
        if (i < (text.length)) {
            // add next character to h1
            document.querySelector(".typetext").innerHTML = text.substring(0, i + 1) + '<span aria-hidden="true" class="blink">|</span>';

            // wait for a while and call this function again for next character
            setTimeout(function () {
                typeWriter(text, i + 1, fnCallback)
            }, 200);
        }
        // text finished, call callback if there is a callback function
        else if (typeof fnCallback == 'function') {
            // call callback after timeout
            setTimeout(fnCallback, 700);
        }
    }
    // start a typewriter animation for a text in the dataText array
    function StartTextAnimation(i) {
        if (typeof dataText[i] == 'undefined') {
            setTimeout(function () {
                StartTextAnimation(0);
            }, 20000);
        }
        // check if dataText[i] exists
        if (i < dataText[i].length) {
            // text exists! start typewriter animation
            typeWriter(dataText[i], 0, function () {
                // after callback (and whole text has been animated), start next text
                StartTextAnimation(i + 1);
            });
        }
    }
    // start the text animation
    StartTextAnimation(0);
});



//"use strict";

//var connection = new signalR.HubConnectionBuilder().withUrl("/notificationhub").build();

//connection.on("newMessage", function (message) {
//    var msg = message.replace(/&/g, "&").replace(/</g, "<").replace(/>/g, ">");
//    var li = document.createElement("li");
//    li.textContent = msg;
//    document.getElementById("messagesList").appendChild(li);
//});

//connection.start().then(function () {
//    var li = document.createElement("li");
//    li.textContent = "Conectado!";
//    document.getElementById("messagesList").appendChild(li);
//}).catch(function (err) {
//    return console.error(err.toString());
//});