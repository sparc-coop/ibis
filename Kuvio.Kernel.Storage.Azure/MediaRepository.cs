using Kuvio.Kernel.Core;
using Kuvio.Kernel.Core.Common;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Storage.Azure
{
    public static class MediaRepositoryExtension
    {
        public static async Task<CloudBlockBlob> GetContainer(this StorageContext context, string containerName, string folderName, string filename, bool overwriteIfFileExists = false)
        {
            filename = Path.GetFileName(filename);

            var url = $"{folderName}/{filename}";
            var blob = context.Container(containerName).GetBlockBlobReference(url);

            if (!overwriteIfFileExists && await blob.ExistsAsync())
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                var extension = Path.GetExtension(filename);

                filename = $"{fileNameWithoutExtension} - {DateTime.UtcNow.ToString("MMM dd yyyy - h:mm:ss")}{extension}";
                url = $"{folderName}/{filename}";
                blob = context.Container(containerName).GetBlockBlobReference(url);
            }

            return blob;
        }
    }

    public class MediaRepository : IMediaRepository
    {
        private readonly StorageContext context;
        private readonly string _storageContainerName;

        public MediaRepository(IConfiguration configuration, StorageContext context)
        {
            this.context = context;
            _storageContainerName = configuration.GetSection("Storage:ContainerName").Value;
        }

        public async Task<Uri> UploadAsync(Stream stream, string folderName, string filename)
        {
            if (String.IsNullOrWhiteSpace(_storageContainerName))
            {
                throw new Exception("You have to set the default Container Name on appsettings: \"Storage:ContainerName\"");
            }

            return await UploadAsync(stream, _storageContainerName, folderName, filename);
        }

        public async Task<Uri> UploadAsync(Stream stream, string containerName, string folderName, string filename)
        {
            stream.Position = 0;
            var containerBlob = await context.GetContainer(containerName, folderName, filename, false);
            await containerBlob.UploadFromStreamAsync(stream);
            return containerBlob.Uri;
        }
    }

    public class MediaRepository<T> : IMediaRepository<T> where T : IFile
    {
        private readonly StorageContext context;
        private readonly string _storageContainerName;

        public MediaRepository(IConfiguration configuration, StorageContext context)
        {
            this.context = context;
            _storageContainerName = configuration.GetSection("Storage:ContainerName").Value;
        }

        public async Task<Uri> UploadAsync(T item)
        {
            item.Stream.Position = 0;
            var containerBlob = await GetContainer(item);
            await containerBlob.UploadFromStreamAsync(item.Stream);
            return containerBlob.Uri;
        }

        private async Task<CloudBlockBlob> GetContainer(T item)
        {
            return await context.GetContainer(_storageContainerName, item.FolderName, item.Filename, false);
        }
    }
}
