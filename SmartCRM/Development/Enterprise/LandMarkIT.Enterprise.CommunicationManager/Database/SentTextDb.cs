using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class SentTextDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TextResponseID { get; set; }
        public Guid Token { get; set; }
        public Guid RequestGuid { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
