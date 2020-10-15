using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kuvio.Kernel.AspNet.Blazor.BlobTransfer
{
    public static class GetBlobFromJavascript
    {
        /// <summary>
        /// <para>You'll have to first load the blob onto a JS variable.</para>
        /// <code>BlazorBlobStream.blobs[blobName] = blobBase64Data;</code>
        /// Then call this method to transfer data from JS to C#
        /// </summary>
        /// <param name="JSRuntime"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        public async static Task<Stream> Execute(IJSRuntime JSRuntime, string blobName)
        {
            return await GetData(JSRuntime, blobName);
        } 

        private static async Task<Stream> GetData(IJSRuntime JSRuntime, string blobName)
        {
            var fileModel = await JSRuntime.InvokeAsync<BlobViewModel>("BlazorBlobStream.execute", blobName);

            if (fileModel == null)
            {
                return null;
            }

            IFileListEntry file = new BlobImpl(JSRuntime, fileModel);

            return file.Data;
        }
    }
}
