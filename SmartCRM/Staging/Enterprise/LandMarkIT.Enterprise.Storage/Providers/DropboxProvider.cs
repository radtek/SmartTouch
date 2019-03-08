using DropNet;
using LandMarkIT.Enterprise.Storage.Core;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace LandMarkIT.Enterprise.Storage.Providers
{
    [Export(typeof(IStorageProvider))]
    public class DropboxProvider : IStorageProvider
    {
        #region Get Client
        private DropNetClient GetClient(StorageCredentials credentials)
        {
            var client = new DropNetClient(credentials.Key, credentials.Secret);
            client.GetAccessToken();
            return client;
        }
        #endregion

        #region Storage Provider
        public List<string> GetFileNames(StorageCredentials credentials, string path)
        {
            return GetClient(credentials).GetMetaData(path).Contents.Select(md => md.Name).ToList();
        }
        public void Upload(StorageCredentials credentials, string path, string fileName, byte[] data)
        {
            GetClient(credentials).UploadFile(path, fileName, data);
        }

        public Stream Download(StorageCredentials credentials, string path, string fileName)
        {
            return new MemoryStream(GetClient(credentials).GetFile(string.Format(@"{0}/{1}", path, fileName)));
        }

        public void Delete(StorageCredentials credentials, string path, string fileName)
        {
            GetClient(credentials).Delete(string.Format(@"{0}/{1}", path, fileName));
        }
        #endregion
    }
}
