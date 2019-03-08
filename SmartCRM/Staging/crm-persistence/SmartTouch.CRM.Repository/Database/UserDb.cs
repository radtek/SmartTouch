using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class UserDb
    {
       [Key]
       public int UserID { get; set; }
       public string UserName { get; set; }
    }
}
