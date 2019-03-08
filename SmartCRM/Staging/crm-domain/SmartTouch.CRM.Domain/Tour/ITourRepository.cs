using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Tour;

namespace SmartTouch.CRM.Domain.Tour
{
    public interface ITourRepository : IRepository<Tour, int>
    {
        IEnumerable<Tour> SearchTour(string name);
        IEnumerable<Tour> FindByContact(int contactId);
        IEnumerable<Tour> FindByTourDate(DateTime tourDate);
        IEnumerable<Tour> FindByCommunity(string communityName);
        IEnumerable<Tour> FindByDetails(string details);

        Tour GetTourByID(int tourId);
        int GetContactTourMapId(int tourId, int contactId);
        int TourContactsCount(int tourId);
        void DeleteTour(int tourId);
    }
}
