using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using System.Reflection;
using SimpleInjector.Integration.Web.Mvc;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Repository.Repositories;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Workflows;

namespace SmartTouch.CRM.SignalR
{
    public static class IoC
    {
        public static Container Container { get; set; }

        public static void Configure(Container container)
        {
            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);

            container.Register<IUserService, UserService>();
            container.Register<IObjectContextFactory, ObjectContextFactory>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IAccountRepository, AccountRepository>();
            container.Register<IUserSettingsRepository, UserSettingsRepository>();
            container.Register<ITourRepository, TourRepository>();
            container.Register<IActionRepository, ActionRepository>();
            container.Register<IUserActivitiesRepository, UserActivitiesRepository>();
            container.Register<IContactRepository, ContactRepository>();
            container.Register<ICachingService, CachingService>();
            container.Register<IWebAnalyticsProviderRepository, WebAnalyticsProviderRepository>();
            container.Register<IRoleRepository, RoleRepository>();
            container.Register<IDropdownRepository, DropdownRepository>();
            container.Register<IUrlService, UrlService>();
            container.Register<ICommunicationProviderService, CommunicationProviderService>();
            container.Register<IServiceProviderRepository, ServiceProviderRepository>();
            container.Register<IMessageRepository, MessageRepository>();


            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();
            /*Create a new SimpleInjectorDependencyResolver that wraps the, 
            container, and register that resolver in MVC.*/
            System.Web.Mvc.DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            //container.Verify();
            IoC.Container = container;
        }
    }
}
