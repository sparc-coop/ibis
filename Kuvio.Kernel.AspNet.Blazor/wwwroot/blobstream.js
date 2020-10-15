(function () {
    window.BlazorBlobStream = {
        elem: {},
        _blobReadPromise: {},
        blobs: {},

        init: function init() {
            // build index by id
            BlazorBlobStream.elem._blazorFilesById = {};
        },

        execute: function execute(id) {
            var blobFile = BlazorBlobStream.blobs[id];

            if (!blobFile) {
                return null;
            }

            //BlazorBlobStream.elem._blazorBlobStreamNextFileId = 0;

            // Reduce to purely serializable data
            BlazorBlobStream._blobReadPromise = null;
            if (!BlazorBlobStream.elem._blazorFilesById) {
                BlazorBlobStream.elem._blazorFilesById = {};
            }

            var result = {
                //id: ++BlazorBlobStream.elem._blazorBlobStreamNextFileId,
                id: id,
                lastModified: new Date().toISOString(),
                name: "somefile",
                size: blobFile.length,
                type: "jpg"
            };
            BlazorBlobStream.elem._blazorFilesById[result.id] = result;

            // Attach the blob data itself as a non-enumerable property so it doesn't appear in the JSON
            Object.defineProperty(result, 'blob', { value: blobFile });
            BlazorBlobStream.blobs[id] = {};

            return result;
        },

        readBlobData: function readBlobData(fileId, startOffset, count) {
            var readPromise = getArrayBufferFromBlobAsync(fileId);

            return readPromise.then(function (arrayBuffer) {
                //var uint8Array = new Uint8Array(arrayBuffer, startOffset, count);
                var uint8Array = arrayBuffer.slice(startOffset, startOffset + count);
                var base64 = uint8ToBase64(uint8Array);
                return base64;
            });
        }
    };

    function getArrayBufferFromBlobAsync(fileId) {
        var file = getBlobById(fileId);

        // On the first read, convert the FileReader into a Promise<ArrayBuffer>
        if (!BlazorBlobStream._blobReadPromise) {
            BlazorBlobStream._blobReadPromise = new Promise(function (resolve, reject) {
                var uint8array = new TextEncoder("utf-8").encode(file.blob);
                resolve(uint8array);
            });
        }

        return BlazorBlobStream._blobReadPromise;
    }

    function getBlobById(fileId) {
        var blob = BlazorBlobStream.elem._blazorFilesById[fileId];
        if (!blob) {
            throw new Error('There is no file with ID ' + fileId + '. The file list may have changed');
        }

        return blob;
    }

    var uint8ToBase64 = (function () {
        // Code from https://github.com/beatgammit/base64-js/
        // License: MIT
        var lookup = [];

        var code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
        for (var i = 0, len = code.length; i < len; ++i) {
            lookup[i] = code[i];
        }

        function tripletToBase64(num) {
            return lookup[num >> 18 & 0x3F] +
                lookup[num >> 12 & 0x3F] +
                lookup[num >> 6 & 0x3F] +
                lookup[num & 0x3F];
        }

        function encodeChunk(uint8, start, end) {
            var tmp;
            var output = [];
            for (var i = start; i < end; i += 3) {
                tmp =
                    ((uint8[i] << 16) & 0xFF0000) +
                    ((uint8[i + 1] << 8) & 0xFF00) +
                    (uint8[i + 2] & 0xFF);
                output.push(tripletToBase64(tmp));
            }
            return output.join('');
        }

        return function fromByteArray(uint8) {
            var tmp;
            var len = uint8.length;
            var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
            var parts = [];
            var maxChunkLength = 16383; // must be multiple of 3

            // go through the array every three bytes, we'll deal with trailing stuff later
            for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
                parts.push(encodeChunk(
                    uint8, i, (i + maxChunkLength) > len2 ? len2 : (i + maxChunkLength)
                ));
            }

            // pad the end with zeros, but make sure to not forget the extra bytes
            if (extraBytes === 1) {
                tmp = uint8[len - 1];
                parts.push(
                    lookup[tmp >> 2] +
                    lookup[(tmp << 4) & 0x3F] +
                    '=='
                );
            } else if (extraBytes === 2) {
                tmp = (uint8[len - 2] << 8) + uint8[len - 1];
                parts.push(
                    lookup[tmp >> 10] +
                    lookup[(tmp >> 4) & 0x3F] +
                    lookup[(tmp << 2) & 0x3F] +
                    '='
                );
            }

            return parts.join('');
        };
    })();
})();
