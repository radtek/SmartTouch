using AutoMapper;
using SmartTouch.CRM.ApplicationServices.Messaging.SeedList;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class SeedEmailService : ISeedEmailService
    {
       readonly ISeedListRepository seedListRepository;
       readonly IUnitOfWork unitOfWork;

       public SeedEmailService(ISeedListRepository seedListRepository, IUnitOfWork unitOfWork)
       {
           if (seedListRepository == null) throw new ArgumentNullException("seedListRepository");
           if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

           this.seedListRepository = seedListRepository;
           this.unitOfWork = unitOfWork;
       }
       public GetSeedListResponse GetSeedList(GetSeedListRquest request)
       {
           GetSeedListResponse response = new GetSeedListResponse();
           IEnumerable<SeedEmail> seedEmails = seedListRepository.GetSeedList();
            
           IEnumerable<SeedEmailViewModel> seedEmailList = Mapper.Map<IEnumerable<SeedEmail>, IEnumerable<SeedEmailViewModel>>(seedEmails);
           response.SeedEmailViewModel = seedEmailList;
           return response;
       }

       public InsertSeedListResponse InsertSeedList(InsertSeedListRequest request)
       {
           InsertSeedListResponse response = new InsertSeedListResponse();
           List<SeedEmail> seedEmails = new List<SeedEmail>();
           if (request.SeedEmailViewModel != null && request.SeedEmailViewModel.Any())
           {
               foreach (var model in request.SeedEmailViewModel)
               {
                   SeedEmail seedEmail = new SeedEmail();
                   seedEmail.Email = model.Email;
                   seedEmails.Add(seedEmail);
               }
           }
           seedListRepository.SaveSeedList(seedEmails,(int)request.RequestedBy);
           return response;
       }
    }
}
