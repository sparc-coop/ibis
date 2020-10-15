using Kuvio.Kernel.Core.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core
{
    public interface IMediaRepository<T> where T : IFile
    {
        Task<Uri> UploadAsync(T item);
    }

    public interface IMediaRepository
    {
        Task<Uri> UploadAsync(Stream stream, string folderName, string filename);
        Task<Uri> UploadAsync(Stream stream, string containerName, string folderName, string filename);
    }
}