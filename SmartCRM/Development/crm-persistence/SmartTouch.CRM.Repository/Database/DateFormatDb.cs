using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class DateFormatDb
    {
        [Key]
        public byte DateFormatId { get; set; }
        public string FormatName { get; set; }
    }
}
