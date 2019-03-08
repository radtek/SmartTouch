using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.Domain.Tours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ITourService
    {
        GetTourListResponse GetTourList(GetTourListRequest request);

        GetTourResponse GetTour(int tourId);
        InsertTourResponse InsertTour(InsertTourRequest request);
        DeleteTourResponse DeleteTour(int tourId, int userId, int contactId);//DeleteContactRequest request
        UpdateTourResponse UpdateTour(UpdateTourRequest updateTourRequest);

        GetContactTourMapResponse GetContactTourMapId(int tourId, int contactId);
        GetTourContactsCountResponse TourContactsCount(int tourId);
        ReIndexDocumentResponse ReIndexTours(ReIndexDocumentRequest request);
        void isTourValid(Tour tour, RequestOrigin? origin);
        GetContactTourIsCreatedResponse IsTourCreate(int contactId);
        GetDashboardChartDetailsResponse GetToursBySourceAreaChartDetails(GetDashboardChartDetailsRequest request);
        GetDashboardChartDetailsResponse GetToursBySourcePieChartDetails(GetDashboardChartDetailsRequest request);
        GetDashboardChartDetailsResponse GetToursByTypeBarChartDetails(GetDashboardChartDetailsRequest request);
        GetDashboardChartDetailsResponse GetToursByTypeFunnelChartDetails(GetDashboardChartDetailsRequest request);
        GetContactsCountResponse TourContactsCount(GetContactsCountRequest request);

        CompletedTourResponse TourStatus(CompletedTourRequest request);
        GetToursToSyncResponse GetToursToSync(GetToursToSyncRequest request);
        GetToursToSyncResponse GetDeletedToursToSync(GetToursToSyncRequest request);
        void TourCompletedTrackmessage(List<int> contactIds,int tourId, short tourType, int accountId, int createdBy);


        Tour UpdateTourBulkData(int tourId, int accountId, int userId, string accountPrimaryEmail, string accountDomain, bool icsCalender, bool icsCalendarToContact, string AccountAddress,IEnumerable<int> contactIDs,bool contactMode,int ownerID,IDictionary<int,Guid> emailGuids, IDictionary<int, Guid> textGuids);
        GetLifeCycleStageofTourResponse GetLifeCycleStageofTour(GetLifeCycleStageofTourRequest request);
    }
}
