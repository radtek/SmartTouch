using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Fields
{
    public class EmailField : Field
    {
        public EmailField()
        {
            this.Title = "Email";
            this.FieldCode = "EmailField";
            this.FieldInputTypeId = Entities.FieldType.datetime;
            }

        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class PrimaryEmailField : EmailField
    {
        public PrimaryEmailField() {
            this.Id = (int)ContactFields.PrimaryEmail;
            this.Title = "Primary Email";
            this.FieldCode = "PrimaryEmailField";
        }


        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class SecondaryEmailField : EmailField
    {

        public SecondaryEmailField()
        {
            this.Id = (int)ContactFields.SecondaryEmail;
            this.Title = "Secondary Email";
            this.FieldCode = "SecondaryEmailField";
        }
       
        protected override void Validate()
        {
            base.Validate();
        }
    }

    public class DonotEmail : EmailField
    {

        public DonotEmail()
        {
            this.Id = (int)ContactFields.DonotEmail;
            this.Title = "Do Not Email";
        }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
