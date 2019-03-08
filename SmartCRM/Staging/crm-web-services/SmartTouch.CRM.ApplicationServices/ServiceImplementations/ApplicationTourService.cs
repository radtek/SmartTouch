using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ApplicationTour;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.TourCMS;
using AutoMapper;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Infrastructure.UnitOfWork;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ApplicationTourService : IApplicationTourService
    {
        readonly IApplicationTourDetailsRepository applicationTourRepo;
        readonly IUnitOfWork unitOfWork;

        public ApplicationTourService(IApplicationTourDetailsRepository applicationTourRepo, IUnitOfWork unitOfWork)
        {
            if (applicationTourRepo == null) throw new ArgumentNullException("applicationTour");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.applicationTourRepo = applicationTourRepo;
            this.unitOfWork = unitOfWork;
        }

        public GetAllTourDetailsResponse FindAll(GetAllTourDetailsRequest request)
        { 
            GetAllTourDetailsResponse response = new GetAllTourDetailsResponse();
            if (request != null)
            {
                var tourDetails = applicationTourRepo.FindAll();
                if (tourDetails != null && tourDetails.Any())
                    response.ApplicationTours = MapDomainToVM(tourDetails);
            }
            return response;
        }

        private IEnumerable<ApplicationTourViewModel> MapDomainToVM(IEnumerable<ApplicationTourDetails> applicationToursDomain)
        {
            List<ApplicationTourViewModel> applicationTours = new List<ApplicationTourViewModel>();
            if (applicationToursDomain != null && applicationToursDomain.Any())
            {
                foreach (var tour in applicationToursDomain)
                {
                    ApplicationTourViewModel details = new ApplicationTourViewModel();
                    details.ApplicationTourDetailsID = tour.ApplicationTourDetailsID;
                    details.DivisionID = tour.DivisionID;
                    details.DivisionName = tour.Division != null ? tour.Division.DivisionName : "";
                    details.SectionID = tour.SectionID;
                    details.SectionName = tour.Section != null ? tour.Section.SectionName : "";
                    details.order = tour.order;
                    details.Title = tour.Title;
                    details.Content = tour.Content;
                    details.HTMLID = tour.HTMLID;
                    details.PopUpPlacement = tour.PopUpPlacement;
                    applicationTours.Add(details);
                }
            }
            return applicationTours;
        }

        public UpdateTourCMSResponse Update(UpdateTourCMSRequest request)
        {
            UpdateTourCMSResponse response = new UpdateTourCMSResponse();
            if (request != null && request.ViewModel != null)
            {
                var domain = Mapper.Map<ApplicationTourViewModel, ApplicationTourDetails>(request.ViewModel);
                applicationTourRepo.Update(domain);
                unitOfWork.Commit();
            }
            return response;
        }

        public UpdateApplicationTourResponse UpdateDetails(UpdateApplicationTourRequest request)
        {
            UpdateApplicationTourResponse response = new UpdateApplicationTourResponse();
            if (request != null)
                applicationTourRepo.UpdateDetails(request.ApplicationTourId, request.Title, request.Content, request.RequestedBy.Value);
            return response;
        }

        public GetByDivisionResponse GetByDivision(GetByDivisionRequest request)
        {
            GetByDivisionResponse response = new GetByDivisionResponse();
            if (request != null && request.DivisionId != 0)
            {
                IEnumerable<ApplicationTourDetails> details = applicationTourRepo.GetByDivision(request.DivisionId);
                if (details != null && details.Any())
                    response.AppTourViewModel = MapDomainToVM(details);
            }
            return response;
        }

        public UpdateTourVisitResponse UpdateTourVisit(UpdateTourVisitRequest request)
        {
            UpdateTourVisitResponse response = new UpdateTourVisitResponse();
            if (request != null)
                applicationTourRepo.UpdateTourVisit(request.UserId);
            return response;
        }
    }
}
