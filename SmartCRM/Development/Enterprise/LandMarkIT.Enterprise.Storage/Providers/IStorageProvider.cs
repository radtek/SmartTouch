using LandMarkIT.Enterprise.Storage.Core;
using System.Collections.Generic;
using System.IO;

namespace LandMarkIT.Enterprise.Storage.Providers
{
    public interface IStorageProvider
    {
        List<string> GetFileNames(StorageCredentials credentials, string path);
        void Upload(StorageCredentials credentials, string path, string fileName, byte[] data);
        Stream Download(StorageCredentials credentials, string path, string fileName);
        void Delete(StorageCredentials credentials, string path, string fileName);
    }
}