// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

function uploadFiles(inputId) {

    console.log('uploadFiles');

    var input = document.getElementById(inputId);
    var files = input.files;
    var form = $('form')[0];
    var formData = new FormData(form);    

    //for (var i = 0; i != files.length; i++) {
    //    formData.append("files", files[i]);
    //}

    console.log('before startUpdatingProgressIndicator');
    startUpdatingProgressIndicator();

    $.ajax(
        {
            url: "/Project",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                stopUpdatingProgressIndicator();
                window.location = '/project-success';
            }
        }
    );
}

var intervalId;

function startUpdatingProgressIndicator() {
    console.log('startUpdatingProgressIndicator');
    $("#progress").show();

    intervalId = setInterval(
        function () {
            // We use the POST requests here to avoid caching problems (we could use the GET requests and disable the cache instead)
            $.post(
                "/Project/Progress",
                function (progress) {
                    $("#bar").css({ width: progress + "%" });
                    $("#label").html(progress + "%");
                }
            );
        },
        1000
    );
}

function stopUpdatingProgressIndicator() {
    clearInterval(intervalId);
}

