using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
  public class SentTextDetailsDb
    {
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int TextResponseDetailsID { get; set; }
      public int TextResponseID { get; set; }
      public string From { get; set; }
      public string To { get; set; }
      public string SenderID { get; set; }
      public string Message { get; set; }
      public byte Status { get; set; }
      public string ServiceResponse { get; set; }
     
    }
}
