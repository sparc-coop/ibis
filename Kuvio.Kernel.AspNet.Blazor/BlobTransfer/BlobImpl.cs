using Microsoft.JSInterop;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kuvio.Kernel.AspNet.Blazor.BlobTransfer
{
    public class BlobViewModel
    {
        public string Id { get; set; }

        public DateTime LastModified { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public string Type { get; set; }
    }

    // This is public only because it's used in a JSInterop method signature,
    // but otherwise is intended as internal
    public class BlobImpl : IFileListEntry
    {
        public int MaxMessageSize { get; set; } = 20 * 1024; // TODO: Use SignalR default
        public int MaxBufferSize { get; set; } = 1024 * 1024;

        private IJSRuntime _jSRuntime;

        public BlobImpl(IJSRuntime jSRuntime, BlobViewModel model)
        {
            _jSRuntime = jSRuntime;
            Id = model.Id;
            Size = model.Size;
            LastModified = model.LastModified;
            Name = model.Name;
        }

        private Stream _stream;

        public event EventHandler OnDataRead;

        public string Id { get; set; }

        public DateTime LastModified { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public string Type { get; set; }

        //public string RelativePath { get; set; }

        public Stream Data
        {
            get
            {
                _stream ??= OpenFileStream(this);
                return _stream;
            }
        }

        internal Stream OpenFileStream(BlobImpl file)
        {
            return new RemoteBlobStream(_jSRuntime, file, MaxMessageSize, MaxBufferSize);
        }

        public async Task<IFileListEntry> ToImageFileAsync(string format, int maxWidth, int maxHeight)
        {
            return null;
            //return await Owner.ConvertToImageFileAsync(this, format, maxWidth, maxHeight);
        }

        internal void RaiseOnDataRead()
        {
            OnDataRead?.Invoke(this, null);
        }
    }
}
