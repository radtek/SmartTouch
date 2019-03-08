using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class EmailStatisticsDb
    {
        [Key]
        public int EmailTrackID  { get; set; }
        public int SentMailDetailID { get; set; }
        public int? ContactID { get; set; }
        public int? EmailLinkID { get; set; }
        public byte ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public string IPAddress { get; set; }
    }
}
