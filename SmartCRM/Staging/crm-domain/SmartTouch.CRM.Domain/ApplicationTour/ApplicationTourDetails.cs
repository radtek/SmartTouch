using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ApplicationTour
{
    public class ApplicationTourDetails : EntityBase<int>, IAggregateRoot
    {
        public int ApplicationTourDetailsID { get; set; }

        public Divisions Division { get; set; }
        public Int16 DivisionID { get; set; } 

        public Sections Section { get; set; }
        public Int16 SectionID { get; set; }

        public Int16 order { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int CreatedBy { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string HTMLID { get; set; }
        public string PopUpPlacement { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
