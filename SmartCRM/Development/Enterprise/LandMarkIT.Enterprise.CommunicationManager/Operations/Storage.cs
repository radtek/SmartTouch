using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;
using System.Collections.Generic;
using System.IO;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class Storage
    {
        private IStorageService GetService(StorageProvider provider, Guid token)
        {
            var service = default(IStorageService);

            switch (provider)
            {
                case StorageProvider.FTP:
                    break;
                case StorageProvider.Dropbox:
                    service = new DropboxService(token);
                    break;
                case StorageProvider.GoogleDrive:
                    break;
                case StorageProvider.SkyDrive:
                    break;
                case StorageProvider.OneDrive:
                    break;
                default:
                    break;
            }
            return service;
        }

        public List<string> GetFileNames(StorageProvider provider, Guid token, string path)
        {
            return GetService(provider, token).GetFileNames(path);
        }
        public void Upload(StorageProvider provider, Guid token, string path, string fileName, byte[] data)
        {
            GetService(provider, token).Upload(path, fileName, data);
        }
        public Stream Download(StorageProvider provider, Guid token, string path, string fileName)
        {
            return GetService(provider, token).Download(path, fileName);
        }
        public void Delete(StorageProvider provider, Guid token, string path, string fileName)
        {
            GetService(provider, token).Download(path, fileName);
        }
    }
}
