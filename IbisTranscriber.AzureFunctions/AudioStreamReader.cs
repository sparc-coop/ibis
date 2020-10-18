using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IbisTranscriber.AzureFunctions
{
    class AudioStreamReader : PullAudioInputStreamCallback
    {
        private BinaryReader _reader;
        public AudioStreamReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }
        public override int Read(byte[] dataBuffer, uint size)
        {
            return _reader.Read(dataBuffer, 0, (int)size);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
                

            if (disposing)
            {
                _reader.Dispose();
            }
                

            _disposed = true;

            base.Dispose(disposing);
        }

        private bool _disposed = false;
    }
}
