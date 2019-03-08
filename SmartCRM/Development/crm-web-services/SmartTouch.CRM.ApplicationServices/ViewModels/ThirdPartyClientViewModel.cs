using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class ThirdPartyClientViewModel
    {        
        public string ID { get; set; }
        public string Name { get; set; }
        
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public bool IsActive { get; set; }
        /// <summary>
        /// In minutes
        /// </summary>
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }        
        public  int? LastUpdatedBy { get; set; }       
        public DateTime LastUpdatedOn { get; set; }
    }
}
