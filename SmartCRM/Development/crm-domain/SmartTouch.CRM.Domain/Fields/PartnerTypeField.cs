using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class PartnerTypeField : Field
    {
        public PartnerTypeField()
        {
            this.Id = (int)ContactFields.PartnerTypeField;
            this.Title = "Partner Type";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class LifecycleStageField : Field
    {
        public LifecycleStageField()
        {
            this.Id = (int)ContactFields.LifecycleStageField;
            this.Title = "Lifecycle Stage";
        }       

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
