using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SmartTouch.CRM.Repository.Database
{
    public class CRMDbInitializer : DropCreateDatabaseIfModelChanges<CRMDb>
    {
        protected override void Seed(CRMDb context)
        {
            base.Seed(context);
        }
    }
}