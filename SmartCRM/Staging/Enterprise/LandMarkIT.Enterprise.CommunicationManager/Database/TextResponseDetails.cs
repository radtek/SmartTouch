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
    [Table("TextResponseDetails")]
    public class TextResponseDetails
    {
        [Key]
        public int TextResponseDetailsID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public string SenderId { get; set; }
        public CommunicationStatus Status { get; set; }
        public string ServiceResponse { get; set; }
        [ForeignKey("TextResponse")]
        public int TextResponseID { get; set; }
        public virtual TextResponseDb TextResponse { get; set; }
    }
}
