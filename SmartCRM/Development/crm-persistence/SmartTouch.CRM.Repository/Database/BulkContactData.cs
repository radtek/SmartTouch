using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class BulkContactData
    {
        [Key]
        public int BulkContactDataID { get; set; }

        public int BulkOperationID { get; set; }
        public int ContactID { get; set; }
    }
}
