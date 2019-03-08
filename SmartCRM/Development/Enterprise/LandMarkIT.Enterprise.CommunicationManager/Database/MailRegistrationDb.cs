using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class MailRegistrationDb
    {
        [Key]
        public int MailRegistrationID { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public string APIKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public bool IsSSLEnabled { get; set; }
        public string VMTA { get; set; }
        public string SenderDomain { get; set; }
        public string ImageDomain { get; set; }
        public MailProvider MailProviderID { get; set; }
        public string MailChimpListID { get; set; }
    }
}
