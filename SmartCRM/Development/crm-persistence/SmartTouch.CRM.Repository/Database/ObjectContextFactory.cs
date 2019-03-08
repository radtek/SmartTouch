using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SmartTouch.CRM.Repository.Database
{
    public class ObjectContextFactory : IObjectContextFactory
    {
        public CRMDb Create()
        {
            //return Nested.instance;
            return new CRMDb();
        }

        private static class Nested
        {
            static Nested() {
                System.Data.Entity.Database.SetInitializer<CRMDb>(null);
                //need to remove this from here and put it in a appropriate place
                //System.Data.Entity.Database.SetInitializer<CRMDb>(new CRMDbInitializer());
            }

            internal static readonly CRMDb instance = new CRMDb();
        }
    }
}
