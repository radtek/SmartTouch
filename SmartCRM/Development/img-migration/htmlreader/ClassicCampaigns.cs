//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace htmlreader
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class ClassicCampaigns
    {
        public int ClassicCampaignID { get; set; }
        public Nullable<decimal> AID { get; set; }
        public Nullable<decimal> cid { get; set; }
        public string HTML { get; set; }
        public Nullable<bool> status { get; set; }
        public string Title { get; set; }
        public Nullable<int> AccountID { get; set; }
        public Nullable<int> CampaignID { get; set; }

        [NotMapped]
        public List<string> InvalidImages { get; set; }
    }
}