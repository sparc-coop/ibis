function speak(audio) {
    const contentType = "audio/mp3";
    const sound = new Howl({
        src: [`data:${contentType};base64,${audio}`]
    });
    sound.play();
}

var context;
var recorder;
async function beginListening(dotNet) {
    try {
        //dotNet.invokeMethodAsync('AudioReceived', createStreamRiffHeader());
        context = new AudioContext();
        recorder = await navigator.mediaDevices.getUserMedia({ audio: true });
        const source = context.createMediaStreamSource(recorder);
        await context.audioWorklet.addModule('/js/ibis-listener.js');
        const processor = new AudioWorkletNode(context, 'ibis-listener');
        source.connect(processor);

        processor.port.onmessage = async e => {
            var data = new Uint8Array(e.data.length);

            for (var i = 0; i < e.data.length; i++) {
                data[i] = e.data[i];
            }
            dotNet.invokeMethodAsync('AudioReceived', data);
        };
    }
    catch (err) {
        console.log(err);
    }
}

async function beginListeningO(dotNet) {
    try {
        recorder = new Recorder({
            encoderPath: '/js/opus-recorder/encoderWorker.min.js',
            encoderApplication: 2048,
            originalSampleRateOverride: 16000,
            encoderSampleRate: 16000,
            //streamPages: true
        });
        
        recorder.ondataavailable = async data => {
            dotNet.invokeMethodAsync('AudioReceived', data);
        }
        recorder.start();
    }
    catch (err) {
        console.log(err);
    }
}

async function beginListeningM(dotNet) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        recorder = new MediaStreamRecorder(stream);
        recorder.mimeType = 'audio/pcm';
        recorder.bitsPerSecond = 16000;
        recorder.audioChannels = 1;
        recorder.ondataavailable = async data => {
            var buffer = await data.arrayBuffer();
            console.log(buffer);
            dotNet.invokeMethodAsync('AudioReceived', new Uint8Array(buffer));
        }
        recorder.start();
    }
    catch (err) {
        console.log(err);
    }
}

async function stopListening() {
    if (recorder) {
        recorder.getTracks().forEach(function (track) {
            track.stop();
        });
    }

    if (context) {
        context.close();
    }
}

window.scrollToBottom = (id) => {
    var div = document.getElementById(id);
    div.scrollTop = div.scrollHeight;
};

window.createStreamRiffHeader = () => {
    //create data buffer
    const buffer = new ArrayBuffer(44);
    const view = new DataView(buffer);

    /* RIFF identifier */
    view.setUint8(0, 'R'.charCodeAt(0));
    view.setUint8(1, 'I'.charCodeAt(0));
    view.setUint8(2, 'F'.charCodeAt(0));
    view.setUint8(3, 'F'.charCodeAt(0));

    /* file length */
    view.setUint32(4, 2 ^ 31, true);
    /* RIFF type & Format */
    view.setUint8(8, 'W'.charCodeAt(0));
    view.setUint8(9, 'A'.charCodeAt(0));
    view.setUint8(10, 'V'.charCodeAt(0));
    view.setUint8(11, 'E'.charCodeAt(0));
    view.setUint8(12, 'f'.charCodeAt(0));
    view.setUint8(13, 'm'.charCodeAt(0));
    view.setUint8(14, 't'.charCodeAt(0));
    view.setUint8(15, ' '.charCodeAt(0));

    /* format chunk length */
    view.setUint32(16, 16, true);
    /* sample format (raw) */
    view.setUint16(20, 1, true);
    /* channel count */
    view.setUint16(22, 1, true);
    /* sample rate */
    view.setUint32(24, 16000, true);
    /* byte rate (sample rate * block align) */
    view.setUint32(28, 32000, true);
    /* block align (channel count * bytes per sample) */
    view.setUint16(32, 2, true);
    /* bits per sample */
    view.setUint16(34, 16, true);
    /* data chunk identifier */
    view.setUint8(36, 'd'.charCodeAt(0));
    view.setUint8(37, 'a'.charCodeAt(0));
    view.setUint8(38, 't'.charCodeAt(0));
    view.setUint8(39, 'a'.charCodeAt(0));

    /* data chunk length */
    view.setUint32(40, 2 ^ 31, true);

    return new Uint8Array(buffer);
}
