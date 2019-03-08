using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Tours
{
    public interface ITourRepository : IRepository<Tour, int>
    {
        IEnumerable<Tour> SearchTour(string name);

        IEnumerable<Tour> FindByContact(int contactId);

        IEnumerable<Tour> FindByTourDate(DateTime tourDate);

        IEnumerable<Tour> FindByCommunity(string communityName);

        IEnumerable<Tour> FindByDetails(string details);

        Tour FindByTourId(int tourid);

        Tour GetTourByID(int tourId);

        int GetContactTourMapId(int tourId, int contactId);

        int TourContactsCount(int tourId);

        IEnumerable<TourContactsSummary> GetTourContactsSummary(int tourId, IEnumerable<int> contactIds,int accountId);

        Dictionary<int, Guid?> DeleteTour(int tourId, int userId, int contactId);

        short[] GetContactComunity(int contactId);

        IEnumerable<DashboardPieChartDetails> ToursBySourcePieChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate);

        IEnumerable<DashboardAreaChart> ToursByLeadsourceAreaChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate);

        IEnumerable<DashboardBarChart> ToursByTypeBarChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate);

        int ContactsCount(int tourId);

        void TourCompleted(int tourId, bool isCompleted, int? contactId, bool CompletedForAll, int userId, DateTime updatedOn);

        IEnumerable<KeyValuePair<int, int>> GetContactCompletedTours(IEnumerable<int> tourIds);

        bool GetTourCompletedStatus(int tourId);

        IEnumerable<Tour> GetToursToSync(int accountId, int userId, int? maxRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType);

        IEnumerable<int> GetDeletedToursToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp);

        void UpdateCRMOutlookMap(Tour tour, RequestOrigin? requestedFrom);

        bool IsTourFromSelectAll(int tourId);
        Guid GetUserEmailGuidByUserId(int userId,int tourId);
        Guid GetUserTextEmailGuid(int userId, int tourId);
        IEnumerable<int> GetAssignedUserIds(int tourId);
        IEnumerable<int> GetContactIds(int tourId);
        IEnumerable<Guid> GetUserEmailGuids(int tourId);
        IEnumerable<Guid> GetUserTextGuids(int tourId);
        void AddingTourDetailsToContactSummary(List<int> tourIds, List<int> contactIds, int accountId, int ownerId);
    }
}
