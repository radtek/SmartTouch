
namespace LandMarkIT.Enterprise.Storage.Core
{
    public class StorageCredentials
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string ApplicationName { get; set; }

        //FTP
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
