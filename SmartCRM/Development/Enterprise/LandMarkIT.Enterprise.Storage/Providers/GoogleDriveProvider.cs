using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using LandMarkIT.Enterprise.Storage.Core;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace LandMarkIT.Enterprise.Storage.Providers
{
    public class GoogleDriveProvider : IStorageProvider
    {
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };
        private const int KB = 0x400;
        private const int DownloadChunkSize = 256 * KB;
        private const string ContentType = @"image/jpeg";

        #region Get Client
        private DriveService GetClient(StorageCredentials credentials)
        {
            using (var stream = GenerateStreamFromString(credentials.Key, credentials.Secret))
            {
                var task = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None);
                task.Wait();
                var userCredential = task.Result;

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = credentials.ApplicationName
                });
            }
        }
        private Stream GenerateStreamFromString(string clientId, string secret)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("\"installed\": {");
            stringBuilder.AppendLine(string.Format("\"client_id\": \"{0}\"", clientId));
            stringBuilder.AppendLine(string.Format("\"client_secret\": \"{0}\"", secret));
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(stringBuilder.ToString());
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        #endregion

        public List<string> GetFileNames(StorageCredentials credentials, string path)
        {
            var fileNames = new List<string>();
            //TODO: implement this
            return fileNames;
        }

        public void Upload(StorageCredentials credentials, string path, string fileName, byte[] data)
        {
            var insert = GetClient(credentials).Files.Insert(new Google.Apis.Drive.v2.Data.File { Title = fileName }, new MemoryStream(data), ContentType);
            insert.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;
            var task = insert.UploadAsync();
            task.Wait();
        }

        public Stream Download(StorageCredentials credentials, string path, string fileName)
        {
            var downloader = new MediaDownloader(GetClient(credentials));
            downloader.ChunkSize = DownloadChunkSize;

            using (var fileStream = new System.IO.FileStream(string.Format(@"{0}/{1}", path, fileName), System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var task = downloader.DownloadAsync(fileName, fileStream);
                task.Wait();
                return fileStream;
            }
        }

        public void Delete(StorageCredentials credentials, string path, string fileName)
        {
            GetClient(credentials).Files.Delete(string.Format(@"{0}/{1}", path, fileName));
        }
    }
}
