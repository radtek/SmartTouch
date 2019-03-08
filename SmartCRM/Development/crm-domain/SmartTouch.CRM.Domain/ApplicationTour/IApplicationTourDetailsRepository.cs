using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ApplicationTour
{
    public interface IApplicationTourDetailsRepository : IRepository<ApplicationTourDetails, int>
    {
        void UpdateDetails(int applicationTourId, string title, string content, int userId);
        IEnumerable<ApplicationTourDetails> GetByDivision(int divisionId);
        void UpdateTourVisit(int userId);
    }
}
