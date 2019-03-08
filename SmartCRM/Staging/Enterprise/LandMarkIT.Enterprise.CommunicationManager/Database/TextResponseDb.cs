using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
    [Table("TextResponse")]
    public class TextResponseDb
    {
        public TextResponseDb()
        {
            TextResponseDetails = new List<TextResponseDetails>();
        }

        [Key]
        public int TextResponseID{get;set;}
        public Guid Token{get;set;}
        public Guid RequestGuid{get;set;}
        public DateTime CreatedDate { get; set; }
        public virtual List<TextResponseDetails> TextResponseDetails { get; set; }
    }
}
