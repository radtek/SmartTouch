using LandMarkIT.Enterprise.Storage.Core;
using Microsoft.Live;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LandMarkIT.Enterprise.Storage.Providers
{
    public class SkyDriveProvider : IStorageProvider
    {
        #region Get Client
        private LiveConnectClient GetClient(StorageCredentials credentials)
        {
            var liveAuthClient = new LiveAuthClient(credentials.Key);
            return new LiveConnectClient(liveAuthClient.Session);
        }
        #endregion

        #region Storage Provider
        public List<string> GetFileNames(StorageCredentials credentials, string path)
        {
            var task = GetClient(credentials).GetAsync(path);
            task.Wait();
            var liveOperationResult = task.Result.Result as IDictionary<string, object>;
            return liveOperationResult.Select(or => or.Key).ToList();
        }
        public void Upload(StorageCredentials credentials, string path, string fileName, byte[] data)
        {
            var task = GetClient(credentials).UploadAsync(path, fileName, new MemoryStream(data), OverwriteOption.Overwrite);
            task.Wait();
        }
        public Stream Download(StorageCredentials credentials, string path, string fileName)
        {
            var task = GetClient(credentials).DownloadAsync(string.Format(@"{0}/{1}", path, fileName));
            task.Wait();
            return task.Result.Stream;
        }
        public void Delete(StorageCredentials credentials, string path, string fileName)
        {
            var task = GetClient(credentials).DeleteAsync(string.Format(@"{0}/{1}", path, fileName));
            task.Wait();
        }
        #endregion
    }
}
