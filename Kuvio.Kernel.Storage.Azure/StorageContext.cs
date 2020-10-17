using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.Storage.Azure
{
    public class StorageContext
    {
        private readonly CloudStorageAccount _storageAccount;

        public StorageContext(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        private readonly Dictionary<string, CloudBlobContainer> BlobReferences =
            new Dictionary<string, CloudBlobContainer>();

        public CloudBlobContainer Container(string containerName)
        {
            if (BlobReferences.ContainsKey(containerName)) return BlobReferences[containerName];

            var context = _storageAccount.CreateCloudBlobClient();
            BlobReferences[containerName] = context.GetContainerReference(containerName);

            return BlobReferences[containerName];
        }
    }
}
