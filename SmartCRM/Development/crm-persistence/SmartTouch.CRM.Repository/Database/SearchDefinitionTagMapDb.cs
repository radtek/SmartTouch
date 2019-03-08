using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SearchDefinitionTagMapDb
    {
        [Key]
        public int SearchDefinitionTagMapID { get; set; }
        [ForeignKey("SearchDefinition")]
        public int SearchDefinitionID { get; set; }
        public virtual SearchDefinitionsDb SearchDefinition { get; set; }
        [ForeignKey("Tags")]
        public int SearchDefinitionTagID { get; set; }
        public virtual TagsDb Tags { get; set; }
    }
}
