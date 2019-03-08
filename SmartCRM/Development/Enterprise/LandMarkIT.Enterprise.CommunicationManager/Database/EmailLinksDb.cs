using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class EmailLinksDb
    {
        [Key]
        public int EmailLinkID  { get; set; }
        public int SentMailDetailID { get; set; }
        public string LinkURL { get; set; }
        public byte LinkIndex { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
