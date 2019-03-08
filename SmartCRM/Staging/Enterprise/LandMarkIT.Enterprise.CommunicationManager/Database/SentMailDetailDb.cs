using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class SentMailDetailDb
    {
        [Key]
        public int SentMailDetailID { get; set; }
        public Guid RequestGuid { get; set; }
        public string DisplayName { get; set; }
        public string ReplyTo { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public Guid? AttachmentGUID { get; set; }
        public Int16 CategoryID { get; set; }
        public int AccountID { get; set; }
    }
}
