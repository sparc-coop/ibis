using System.IO;

namespace Kuvio.Kernel.Core.Common
{
    public interface IFile {
        string Filename { get; }
        string FolderName { get; }
        Stream Stream { get; }
    }
}