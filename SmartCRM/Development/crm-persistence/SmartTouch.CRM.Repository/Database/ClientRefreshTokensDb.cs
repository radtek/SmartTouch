using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ClientRefreshTokensDb
    {
        [Key]
        public string Id { get; set; }
        public int IssuedTo { get; set; }
        [ForeignKey("IssuedTo")]
        public UsersDb IssuedToUser { get; set; }
        public string ThirdPartyClientId { get; set; }
        [ForeignKey("ThirdPartyClientId")]
        public ThirdPartyClientsDb Client { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string ProtectedTicket { get; set; }
        public int LastUpdatedBy { get; set; }
        [ForeignKey("LastUpdatedBy")]
        public UsersDb LastUpdatedByUser { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
