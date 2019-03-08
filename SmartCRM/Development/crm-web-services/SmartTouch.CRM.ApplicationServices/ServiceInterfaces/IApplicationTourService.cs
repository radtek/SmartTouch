using SmartTouch.CRM.ApplicationServices.Messaging.TourCMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IApplicationTourService
    {
        GetAllTourDetailsResponse FindAll(GetAllTourDetailsRequest request);
        UpdateTourCMSResponse Update(UpdateTourCMSRequest request);
        UpdateApplicationTourResponse UpdateDetails(UpdateApplicationTourRequest request);
        GetByDivisionResponse GetByDivision(GetByDivisionRequest request);
        UpdateTourVisitResponse UpdateTourVisit(UpdateTourVisitRequest request);
    }
}
