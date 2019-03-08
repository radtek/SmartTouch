using LandMarkIT.Enterprise.Storage.Core;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LandMarkIT.Enterprise.Storage.Providers
{
    public class FtpProvider : IStorageProvider
    {
        #region Get Client
        private FtpWebRequest GetClient(StorageCredentials credentials, string path)
        {
            var request = WebRequest.Create(string.Concat(credentials.Host, "/", path)) as FtpWebRequest;
            request.Credentials = new NetworkCredential(credentials.UserName, credentials.Password);

            return request;
        }
        #endregion

        #region Storage Provider
        public List<string> GetFileNames(StorageCredentials credentials, string path)
        {
            var result = new List<string>();

            var client = GetClient(credentials, path);
            client.Method = WebRequestMethods.Ftp.ListDirectory;

            var response = (FtpWebResponse)client.GetResponse();
            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);

            while (!reader.EndOfStream)
            {
                result.Add(reader.ReadLine());
            }

            //Clean-up
            reader.Close();
            responseStream.Close(); //redundant
            response.Close();

            return result;
        }
        public void Upload(StorageCredentials credentials, string path, string fileName, byte[] data)
        {
            var client = GetClient(credentials, string.Concat(path, "/", fileName));
            client.Method = WebRequestMethods.Ftp.UploadFile;
            client.ContentLength = data.Length;

            var requestStream = client.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            var response = (FtpWebResponse)client.GetResponse();
        }

        public Stream Download(StorageCredentials credentials, string path, string fileName)
        {
            var client = GetClient(credentials, string.Concat(path, "/", fileName));
            client.Method = WebRequestMethods.Ftp.DownloadFile;

            return ((FtpWebResponse)client.GetResponse()).GetResponseStream();
        }

        public void Delete(StorageCredentials credentials, string path, string fileName)
        {
            var client = GetClient(credentials, string.Concat(path, "/", fileName));
            client.Method = WebRequestMethods.Ftp.DeleteFile;
            var response = (FtpWebResponse)client.GetResponse();            
            response.Close();
        }
        #endregion
    }
}
