using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
     [Table("MailResponse")]
    public class MailResponse
    {
        public MailResponse()
        {
            MailResponseDetails = new List<MailResponseDetails>();
        }
        [Key]
        public int MailResponseID { get; set; }
        public Guid Token { get; set; }
        public Guid RequestGuid { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<MailResponseDetails> MailResponseDetails { get; set; }
    }
}
