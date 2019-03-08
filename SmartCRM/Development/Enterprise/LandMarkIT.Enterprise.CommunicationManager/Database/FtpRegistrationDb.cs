using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class FtpRegistrationDb
    {
        [Key]
        public int FtpRegistrationID { get; set; }
        public Guid Guid { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}
