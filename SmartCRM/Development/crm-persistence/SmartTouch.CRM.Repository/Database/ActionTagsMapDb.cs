using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class ActionTagsMapDb
    {
        [Key]
        public int ActionTagMapID { get; set; }
        
        public int ActionID { get; set; }
        [ForeignKey("ActionID")]
        public ActionsDb Action { get; set; }
        
        public int TagID { get; set; }
        [ForeignKey("TagID")]
        public TagsDb Tag { get; set; }
    }
}
