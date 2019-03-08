using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SeedList
{
    public interface ISeedListRepository : IRepository<SeedEmail, int>
    {
        IEnumerable<SeedEmail> GetSeedList();
        void SaveSeedList(IEnumerable<SeedEmail> seedEmail,int userId);
    }
}
