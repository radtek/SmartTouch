using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
   [Table("MailResponseDetails")]
   public class MailResponseDetails
    {
       public MailResponseDetails()
       {
           MailResponse = new MailResponse();
       }
       [Key]
       public int MailResponseDetailsID { get; set; }
       public Guid MailGuid { get; set; }
       public string To { get; set; }
       public string From { get; set; }
       public string CC { get; set; }
       public string BCC { get; set; }
       public CommunicationStatus Status { get; set; }
       public string ServiceResponse { get; set; }
       [ForeignKey("MailResponse")]
       public int MailResponseID { get; set; }
       public virtual MailResponse MailResponse { get; set; }
       public DateTime CreatedDate { get; set; }
    }
}
