class IbisListener extends AudioWorkletProcessor {
    bufferSize = 4096
    _bytesWritten = 0

    _buffer = new Float32Array(this.bufferSize)

    constructor() {
        super()
        this.initBuffer()
    }

    initBuffer() {
        this._bytesWritten = 0
    }

    isBufferEmpty() {
        return this._bytesWritten === 0
    }

    isBufferFull() {
        return this._bytesWritten >= this.bufferSize
    }

    flush() {
        this.port.postMessage(
            this._bytesWritten < this.bufferSize
                ? this._buffer.slice(0, this._bytesWritten)
                : this._buffer
        )
        this.initBuffer()
    }

    process(inputs) {
        this.append(inputs[0][0])
        return true;
    }

    append(channelData) {
        if (this.isBufferFull()) {
            this.flush()
        }

        if (!channelData) return;

        const encodedData = new Uint8Array(this.encode(channelData));

        for (let i = 0; i < encodedData.byteLength; i++) {
            this._buffer[this._bytesWritten++] = encodedData[i];
        }
    }

    encode(data) {
        const audioFrame = this.downSampleAudioFrame(data, sampleRate, 16000);

        if (!audioFrame) {
            return null;
        }

        const audioLength = audioFrame.length * 2;

        const buffer = new ArrayBuffer(audioLength);
        const view = new DataView(buffer);
        this.floatTo16BitPCM(view, 0, audioFrame);
        return buffer;
    }

    floatTo16BitPCM(view, offset, input) {
        for (let i = 0; i < input.length; i++, offset += 2) {
            const s = Math.max(-1, Math.min(1, input[i]));
            view.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7FFF, true);
        }
    }

    downSampleAudioFrame(
        srcFrame,
        srcRate,
        dstRate) {

        if (!srcFrame) {
            return null;
        }

        if (dstRate === srcRate || dstRate > srcRate) {
            return srcFrame;
        }

        const ratio = srcRate / dstRate;
        const dstLength = Math.round(srcFrame.length / ratio);
        const dstFrame = new Float32Array(dstLength);
        let srcOffset = 0;
        let dstOffset = 0;
        while (dstOffset < dstLength) {
            const nextSrcOffset = Math.round((dstOffset + 1) * ratio);
            let accum = 0;
            let count = 0;
            while (srcOffset < nextSrcOffset && srcOffset < srcFrame.length) {
                accum += srcFrame[srcOffset++];
                count++;
            }
            dstFrame[dstOffset++] = accum / count;
        }

        return dstFrame;
    }
}
    
registerProcessor('ibis-listener', IbisListener);