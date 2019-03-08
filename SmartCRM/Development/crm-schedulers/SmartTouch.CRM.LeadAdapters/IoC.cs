using SimpleInjector;
using SimpleInjector.Extensions.LifetimeScoping;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Repository.Repositories;

namespace SmartTouch.CRM.LeadAdapters
{
    public static class IoC
    {
        public static Container Container { get; set; }

        public static void Configure(Container container)
        {
            var lifetimeScope = new LifetimeScopeLifestyle();

            //container.Register<IUnitOfWork, UnitOfWork>(lifetimeScope);

            //container.RegisterManyForOpenGeneric(
            //    typeof(IRepository<>),
            //    lifetimeScope,
            //    typeof(IRepository<>).Assembly);
            container.Register<IActionRepository, ActionRepository>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IObjectContextFactory, ObjectContextFactory>();
            container.Register<IContactRepository, ContactRepository>();
            container.Register<ITagRepository, TagRepository>();
            container.Register<IAccountRepository, AccountRepository>();
            container.Register<ITourRepository, TourRepository>();
            container.Register<INoteRepository, NoteRepository>();
            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);
            container.Register<ICommunicationRepository, CommunicationRepository>();
           

            container.Register<IAttachmentRepository, AttachmentRepository>();


            container.Register<ICampaignRepository, CampaignRepository>();
            
            container.Register<ICommunicationLogInDetailRepository, CommunicationLogInDetailRepository>();

            //var services = GlobalConfiguration.Configuration.Services;
            //var controllerTypes = services.GetHttpControllerTypeResolver()
            //    .GetControllerTypes(services.GetAssembliesResolver());

            //foreach (var controllerType in controllerTypes)
            //{
            //    container.Register(controllerType);
            //}

            //container.Verify();
            //GlobalConfiguration.Configuration.DependencyResolver =
            //        new SimpleInjectorWebApiDependencyResolver(container);
            IoC.Container = container;
        }
    }
}
