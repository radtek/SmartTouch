using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class NeverBounceMappingsDb
    {
        [Key]
        public int NeverBounceMappingID { get; set; }

        [ForeignKey("NeverBounceRequest")]
        public virtual int NeverBounceRequestID { get; set; }
        public virtual NeverBounceRequestDb NeverBounceRequest { get; set; }

        public int EntityID { get; set; }
        public byte NeverBounceEntityType { get; set; }
    }
}
